using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pets_Enhanced_Mod.RefAPIs
{
    public class SymaHatsOnPetsPlusData
    {
        public class ExternalPetModData
        {
            /// <summary>Defines the type of pet. Main vanilla types are "Dog", "Cat", "Turtle"</summary>
            public string Type { get; set; }

            /// <summary>Defines the breed of the pet. Breeds are usually numbered 0 to 4, except for turtles that are 0 and 1 only"</summary>
            public string? BreedId { get; set; }

            /// <summary>Defines a list of breeds that use these data."</summary>
            public string[]? BreedIdList { get; set; }

            /// <summary>Defines a list of sprites that help position the hat correctly."</summary>
            public ExternalSpriteModData[] Sprites { get; set; }
        }

        public class ExternalSpriteModData
        {
            public int SpriteId { get; set; }

            public float? HatOffsetX { get; set; }

            public float? HatOffsetY { get; set; }

            public int? Direction { get; set; }

            public float? Scale { get; set; }

            public bool? Flipped { get; set; }

            public bool? Default { get; set; }

            public bool? DoNotDrawHat { get; set; }
        }
    }
}
