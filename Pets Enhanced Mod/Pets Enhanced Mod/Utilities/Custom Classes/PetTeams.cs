using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley.Characters;
using StardewValley;
using StardewModdingAPI;
using System.Threading.Tasks;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static Pets_Enhanced_Mod.Utilities.SunkenLaceUtilities;
using StardewValley.Extensions;
using StardewValley.Monsters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Pets_Enhanced_Mod.Utilities.Custom_Classes.SmartPet;
using Pets_Enhanced_Mod.Multiplayer;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public class PetTeams
    {
        public PetTeams()
        {

        }
        public class PetTeam
        {
            public readonly struct KeyValuePairFloatComparer : IComparer<KeyValuePair<float, Guid>>
            {
                public int Compare(KeyValuePair<float, Guid> x, KeyValuePair<float, Guid> y)
                {
                    return x.Key.CompareTo(y.Key);
                }
            }
            private Farmer Leader { get; set; }

            public Guid? Follower1;
            public Guid? Follower2;
            public Guid? Follower3;

            public List<string> CollarsWornByTeam = new();
            public bool IsTeamFull() => this.Follower1 is not null && this.Follower2 is not null && this.Follower3 is not null;
            /// <summary>Checks whether this instance is already on the team.</summary>
            /// <returns>Whether the instance is the same or not.</returns>
            public bool IsFollowerAlreadyPresent(Guid petID) => (this.Follower1 is not null && this.Follower1 == petID) || (this.Follower2 is not null && this.Follower2 == petID) || (this.Follower3 is not null && this.Follower3 == petID);

            public int GetFollowerIndexByID(Guid petID) => (this.Follower1 is not null && this.Follower1 == petID) ? 0 : (this.Follower2 is not null && this.Follower2 == petID) ? 1 : (this.Follower3 is not null && this.Follower3 == petID) ? 2 : -1;

            public bool IsTeamEmpty() => this.Follower1 is null && this.Follower2 is null && this.Follower3 is null;
            /// <summary>Gets the spots available in the team.</summary>
            public int GetAvailableSpaceInTeam()
            {
                int s = 3;
                if (this.Follower1 is not null)
                {
                    s--;
                }
                if (this.Follower2 is not null)
                {
                    s--;
                }
                if (this.Follower3 is not null)
                {
                    s--;
                }
                return s;
            }
            public bool TryAddPetToTeam(Guid petID)
            {
                if (this.Follower1 is null)
                {
                    this.Follower1 = petID;
                    return true;
                }
                if (this.Follower2 is null)
                {
                    this.Follower2 = petID;
                    return true;
                }
                if (this.Follower3 is null)
                {
                    this.Follower3 = petID;
                    return true;
                }
                return false;
            }
            /// <summary>Tries to remove the specified pet instance from the team if it is present.</summary>
            /// <returns>Whether the pet has been successfully removed from team.</returns>
            public bool TryRemovePetFromTeam(Guid? petID)
            {
                if (petID is null) {  return false; }
                int i = GetFollowerIndexByID(petID.Value);
                if (i > -1 && CachePetData.PetCache.TryGetValue(petID.Value, out var data))
                {
                    data.Pet.PetLeader = null;
                    data.Pet.HumanLeader = null;
                    data.Pet.GroupLeader = null;
                    if (i == 0) { this.Follower1 = this.Follower2; this.Follower2 = this.Follower3; this.Follower3 = null; }
                    else if (i == 1) { this.Follower2 = this.Follower3; this.Follower3 = null; }
                    else if (i == 2) { this.Follower3 = null; }

                    return true;
                }
                return false;
            }
            /// <summary>Reorganizes the team based on the distance between its members and the leader, and by whether they're doing something else other than just following.</summary>
            private void ReorganizeTeam() //488 bytes
            {
                (Guid id,float distance,bool following)? _follower1 = null;
                (Guid id, float distance, bool following)? _follower2 = null;
                (Guid id, float distance, bool following)? _follower3 = null;
                if (this.Follower1 is not null && CachePetData.PetCache.TryGetValue(this.Follower1.Value, out var f1Data))
                {
                    _follower1 = (this.Follower1.Value, f1Data.Pet.DistanceFromGroupLeader, f1Data.Pet.CurrentPetAction == SmartPet.PetAction.Follow);
                }
                if (this.Follower2 is not null && CachePetData.PetCache.TryGetValue(this.Follower2.Value, out var f2Data))
                {
                    _follower2 = (this.Follower2.Value, f2Data.Pet.DistanceFromGroupLeader, f2Data.Pet.CurrentPetAction == SmartPet.PetAction.Follow);
                }
                if (this.Follower3 is not null && CachePetData.PetCache.TryGetValue(this.Follower3.Value, out var f3Data))
                {
                    _follower3 = (this.Follower3.Value, f3Data.Pet.DistanceFromGroupLeader, f3Data.Pet.CurrentPetAction == SmartPet.PetAction.Follow);
                }
                int maxAttempts = 4;
                bool _ordered = false;
                for (int i = 0; i < maxAttempts && !_ordered; i++)
                {
                    _ordered = true;
                    if (_follower1.HasValue && _follower2.HasValue && ((_follower1.Value.distance > _follower2.Value.distance && _follower1.Value.following == _follower2.Value.following) || (!_follower1.Value.following && _follower2.Value.following)))
                    {
                        (_follower1, _follower2) = (_follower2, _follower1);
                        _ordered = false;
                    }
                    if (_follower2.HasValue && _follower3.HasValue && ((_follower2.Value.distance > _follower3.Value.distance && _follower2.Value.following == _follower3.Value.following) || (!_follower2.Value.following && _follower3.Value.following)))
                    {
                        (_follower2, _follower3) = (_follower3, _follower2);
                        _ordered = false;
                    }
                }
                this.Follower1 = _follower1.HasValue? _follower1.Value.id : null;
                this.Follower2 = _follower2.HasValue ? _follower2.Value.id : null;
                this.Follower3 = _follower3.HasValue ? _follower3.Value.id : null;
            }
            private List<KeyValuePair<float, Guid>> GetNotFollowingFollowersAsListOfDistanceFromLeader(List<KeyValuePair<float, Guid>> _listForUse)
            {
                _listForUse.Clear();
                if (this.Follower1 is not null && CachePetData.PetCache.TryGetValue(this.Follower1.Value, out var f1Data) && f1Data.Pet.CurrentPetAction != SmartPet.PetAction.Follow)
                {
                    _listForUse.Add(new(f1Data.Pet.DistanceFromGroupLeader, this.Follower1.Value));
                }
                if (this.Follower2 is not null && CachePetData.PetCache.TryGetValue(this.Follower2.Value, out var f2Data) && f2Data.Pet.CurrentPetAction != SmartPet.PetAction.Follow)
                {
                    _listForUse.Add(new(f2Data.Pet.DistanceFromGroupLeader, this.Follower2.Value));
                }
                if (this.Follower3 is not null && CachePetData.PetCache.TryGetValue(this.Follower3.Value, out var f3Data) && f3Data.Pet.CurrentPetAction != SmartPet.PetAction.Follow)
                {
                    _listForUse.Add(new(f3Data.Pet.DistanceFromGroupLeader, this.Follower3.Value));
                }
                return _listForUse;
            }
            private List<KeyValuePair<float, Guid>> GetJustFollowingFollowersAsListOfDistanceFromLeader(List<KeyValuePair<float, Guid>> _listForUse)
            {
                _listForUse.Clear();
                if (this.Follower1 is not null && CachePetData.PetCache.TryGetValue(this.Follower1.Value, out var f1Data) && f1Data.Pet.CurrentPetAction == SmartPet.PetAction.Follow)
                {
                    _listForUse.Add(new(f1Data.Pet.DistanceFromGroupLeader, this.Follower1.Value));
                }
                if (this.Follower2 is not null && CachePetData.PetCache.TryGetValue(this.Follower2.Value, out var f2Data) && f2Data.Pet.CurrentPetAction == SmartPet.PetAction.Follow)
                {
                    _listForUse.Add(new(f2Data.Pet.DistanceFromGroupLeader, this.Follower2.Value));
                }
                if (this.Follower3 is not null && CachePetData.PetCache.TryGetValue(this.Follower3.Value, out var f3Data) && f3Data.Pet.CurrentPetAction == SmartPet.PetAction.Follow)
                {
                    _listForUse.Add(new(f3Data.Pet.DistanceFromGroupLeader, this.Follower3.Value));
                }
                return _listForUse;
            }
            public void UpdateTeam() //624 bytes
            {
                this.CollarsWornByTeam.Clear();
                if (GetAvailableSpaceInTeam() >= 3) { return; }
                ReorganizeTeam();
                if (this.Follower1 is not null && CachePetData.PetCache.TryGetValue(this.Follower1.Value, out var f1Data))
                {
                    f1Data.Pet.HumanLeader = f1Data.Pet.GroupLeader = this.Leader;
                    f1Data.Pet.PetLeader = null;
                    f1Data.Pet.Teammate1ID = this.Follower2;
                    f1Data.Pet.Teammate2ID = this.Follower3;
                    if (!SynchronizationManager.TryGetPetAccessory(f1Data.Pet.OriginalPetInstance).IsNull(out var f1DataCollar))
                    {
                        this.CollarsWornByTeam.Add(f1DataCollar.QualifiedItemId);
                    }
                }
                if (this.Follower2 is not null && CachePetData.PetCache.TryGetValue(this.Follower2.Value, out var f2Data))
                {
                    f2Data.Pet.HumanLeader = null;
                    f2Data.Pet.PetLeader = this.Follower1 is not null && CachePetData.PetCache.TryGetValue(this.Follower1.Value, out var fs) ? fs.Pet : null;
                    f2Data.Pet.Teammate1ID = this.Follower3;
                    f2Data.Pet.Teammate2ID = this.Follower1;
                    if (!SynchronizationManager.TryGetPetAccessory(f2Data.Pet.OriginalPetInstance).IsNull(out var f2DataCollar))
                    {
                        this.CollarsWornByTeam.Add(f2DataCollar.QualifiedItemId);
                    }
                }
                if (this.Follower3 is not null && CachePetData.PetCache.TryGetValue(this.Follower3.Value, out var f3Data))
                {
                    f3Data.Pet.HumanLeader = null;
                    f3Data.Pet.PetLeader = this.Follower2 is not null && CachePetData.PetCache.TryGetValue(this.Follower2.Value, out var fs) ? fs.Pet : null;
                    f3Data.Pet.Teammate1ID = this.Follower1;
                    f3Data.Pet.Teammate2ID = this.Follower2;
                    if (!SynchronizationManager.TryGetPetAccessory(f3Data.Pet.OriginalPetInstance).IsNull(out var f3DataCollar))
                    {
                        this.CollarsWornByTeam.Add(f3DataCollar.QualifiedItemId);
                    }
                }
            }
            public PetTeam(Farmer leader, Guid firstFollower)
            {
                this.Leader = leader;
                this.Follower1 = firstFollower;
                this.UpdateTeam();
            }
        }
        public Dictionary<long, PetTeam> Teams = new();

        private readonly List<Rectangle> TerrainFeatureIgnoreList = new();

        private readonly Dictionary<Monster, Guid> TargetedMonsters = new();

        private readonly Dictionary<Rectangle, Guid> TargetedTerrainFeatures = new();
        public void Clear()
        {
            this.Teams.Clear();
            this.TerrainFeatureIgnoreList.Clear();
            this.TargetedMonsters.Clear();
            this.TargetedTerrainFeatures.Clear();
        }
        public void CleanUpTeams()
        {
            if (Teams.Count > 0)
            {
                List<long> _list = CacheReciclerHelper.RentLong();
                foreach (var team in Teams)
                {
                    if (team.Value.IsTeamEmpty())
                    {
                        _list.Add(team.Key);
                    }
                }
                for (int i = 0; i < _list.Count; i++)
                {
                    Teams.Remove(_list[i]);
                }
                CacheReciclerHelper.Return(_list);
            }
        }
        public void UpdateTeams()
        {
            CleanUpTeams();

            foreach (var team in Teams)
            {
                team.Value.UpdateTeam();
            }
            
        }
        public bool IsFarmerAlreadyLeaderOnTeam(long farmerID) => Teams.ContainsKey(farmerID);
        public bool IsPetAlreadyOnATeam(Guid petID) 
        { 
            foreach (var team in Teams)
            {
                if (team.Value.IsFollowerAlreadyPresent(petID))
                {
                    return true;
                }
            }
            return false;
        }

        public long? GetLeaderOfPetIfAny(Guid petID)
        {
            foreach (var team in Teams)
            {
                if (team.Value.IsFollowerAlreadyPresent(petID)) { return team.Key; }
            }
            return null;
        }
        public Guid? IsTargetAnotherPetTarget(Monster target)
        {
            if (target is not null && TargetedMonsters.TryGetValue(target, out Guid result))
            {
                return result;
            }
            return null;
        }
        public List<string> GetCollarsWornByTeam(long leaderID)
        {
            if (this.Teams.TryGetValue(leaderID, out var team))
            {
                return team.CollarsWornByTeam;
            }
            return new();
        }
        public bool TryAddTargetToTargetedMonsters(Monster target, Guid who)
        {
            if (target is null) { return false; }
            return TargetedMonsters.TryAdd(target, who);
        }
        public bool TryRemoveTargetToTargetedMonsters(Monster target)
        {
            if (target is null) { return false; }
            return TargetedMonsters.Remove(target);
        }
        public bool IsTargetTerrainFeatureAnotherPetTarget(Rectangle target, Guid petAsking)
        {
            if (TargetedTerrainFeatures.TryGetValue(target, out Guid result))
            {
                return result != petAsking;
            }
            return false;
        }
        public bool TryAddTargetTerrainFeatureToTargetedTerrainFeatures(Rectangle target, Guid who)
        {
            return TargetedTerrainFeatures.TryAdd(target, who);
        }
        public bool TryRemoveTargetTerrainFeatureToTargetedTerrainFeatures(Rectangle target)
        {
            return TargetedTerrainFeatures.Remove(target);
        }
        public bool IsTerrainFeatureOnTFIgnoreList(Rectangle tf) => this.TerrainFeatureIgnoreList.Contains(tf);
        public bool AddTerrainFeatureToIgnoreList(Rectangle tf)
        {
            if (!this.TerrainFeatureIgnoreList.Contains(tf))
            {
                this.TerrainFeatureIgnoreList.Add(tf);
                return true;
            }
            return false;
        }
        public bool CreateNewTeam(Farmer groupLeader, Guid firstMemberID)
        {
            if (!IsPetAlreadyOnATeam(firstMemberID) && !IsFarmerAlreadyLeaderOnTeam(groupLeader.UniqueMultiplayerID))
            {
                Teams.Add(groupLeader.UniqueMultiplayerID, new PetTeam(groupLeader, firstMemberID));
                return true;
            }
            return false;
        }
        public void returnTeamWhenLeaderDisconected(long leaderID)
        {
            if (Teams.TryGetValue(leaderID, out var team))
            {
                Guid? followerOne = team.Follower1;
                Guid? followerTwo = team.Follower2;
                Guid? followerThree = team.Follower3;
                if (followerOne is not null)
                {
                    if (CachePetData.PetCache.TryGetValue(followerOne.Value, out var pKit))
                    {
                        pKit.Pet.SetPetObjective(SmartPet.PetObjective.None, Game1.player);
                        pKit.Pet.ResetVariables();
                        pKit.Pet.SetOPetAtFarmPosition();
                        pKit.Pet.SetSPetToCopyOPet();

                    }
                }
                if (followerTwo is not null)
                {
                    if (CachePetData.PetCache.TryGetValue(followerTwo.Value, out var pKit2))
                    {
                        pKit2.Pet.SetPetObjective(SmartPet.PetObjective.None, Game1.player);
                        pKit2.Pet.ResetVariables();
                        pKit2.Pet.SetOPetAtFarmPosition();
                        pKit2.Pet.SetSPetToCopyOPet();

                    }
                }
                if (followerThree is not null)
                {
                    if (CachePetData.PetCache.TryGetValue(followerThree.Value, out var pKit3))
                    {
                        pKit3.Pet.SetPetObjective(SmartPet.PetObjective.None, Game1.player);
                        pKit3.Pet.ResetVariables();
                        pKit3.Pet.SetOPetAtFarmPosition();
                        pKit3.Pet.SetSPetToCopyOPet();

                    }
                }
            }
        }
        public void returnPetFromTeam(long leaderId, Guid _petID)
        {
            if (Teams.TryGetValue(leaderId, out var team))
            {
                if (team.Follower1 == _petID)
                {
                    if (CachePetData.PetCache.TryGetValue(_petID, out var pKit))
                    {
                        pKit.Pet.SetPetObjective(SmartPet.PetObjective.None, Game1.player);
                        pKit.Pet.ResetVariables();
                        pKit.Pet.SetOPetAtFarmPosition();
                        pKit.Pet.SetSPetToCopyOPet();
                    }
                }
                else if (team.Follower2 == _petID)
                {
                    if (CachePetData.PetCache.TryGetValue(_petID, out var pKit2))
                    {
                        pKit2.Pet.SetPetObjective(SmartPet.PetObjective.None, Game1.player);
                        pKit2.Pet.ResetVariables();
                        pKit2.Pet.SetOPetAtFarmPosition();
                        pKit2.Pet.SetSPetToCopyOPet();
                    }
                }
                else if (team.Follower3 == _petID)
                {
                    if (CachePetData.PetCache.TryGetValue(_petID, out var pKit3))
                    {
                        pKit3.Pet.SetPetObjective(SmartPet.PetObjective.None, Game1.player);
                        pKit3.Pet.ResetVariables();
                        pKit3.Pet.SetOPetAtFarmPosition();
                        pKit3.Pet.SetSPetToCopyOPet();
                    }
                }
            }
        }
        public bool IsTeamFull(long groupLeader)
        {
            if (Teams.TryGetValue(groupLeader, out var t) && t.IsTeamFull())
            {
                return true;
            }
            return false;
        }

        public bool RelocatePetToTeamOtherwiseCreateNew(Guid pet, long groupLeader)
        {
            if (IsPetAlreadyOnATeam(pet))
            {
                var leaderID = GetLeaderOfPetIfAny(pet);
                if (leaderID == groupLeader)
                {
                    return true;
                }
                else
                {
                    RemovePetFromTeam(pet, leaderID);

                    if (Teams.TryGetValue(groupLeader, out var t))
                    {
                        return t.TryAddPetToTeam(pet);
                    }
                    return CreateNewTeam(Game1.GetPlayer(groupLeader), pet);
                }
            }
            else
            {
                if (Teams.TryGetValue(groupLeader, out var t))
                {
                    return t.TryAddPetToTeam(pet);
                }
                return CreateNewTeam(Game1.GetPlayer(groupLeader), pet);
            }
        }

        public bool RemovePetFromTeam(Guid pet, long? groupLeader)
        {
            if (groupLeader is not null && Teams.TryGetValue(groupLeader.Value, out var t))
            {
                return t.TryRemovePetFromTeam(pet);
            }
            return false;
        }
    }


}
