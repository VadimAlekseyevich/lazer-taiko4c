// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Hit : TaikoStrongableHitObject, IHasDisplayColour
    {
        private HitObjectProperty<HitType> type;
        private HitHand? displayHand;

        public Bindable<HitType> TypeBindable => type.Bindable;

        /// <summary>
        /// The <see cref="HitType"/> that actuates this <see cref="Hit"/>.
        /// </summary>
        public HitType Type
        {
            get => type.Value;
            set => type.Value = value;
        }

        /// <summary>
        /// The hand this hit should be visually played with.
        /// </summary>
        public HitHand? DisplayHand
        {
            get => displayHand;
            set
            {
                displayHand = value;
                updateDisplayColour();
            }
        }

        public Bindable<Color4> DisplayColour { get; } = new Bindable<Color4>(COLOUR_CENTRE);

        public static readonly Color4 COLOUR_CENTRE = Color4Extensions.FromHex(@"2299bb");
        public static readonly Color4 COLOUR_RIM = Color4Extensions.FromHex(@"f28c28");

        // Four-colour mapping. The original blue/orange pair is retained for one hand,
        // with green/purple used for the alternating hand.
        public static readonly Color4 COLOUR_CENTRE_LEFT = Color4Extensions.FromHex(@"38c172");
        public static readonly Color4 COLOUR_CENTRE_RIGHT = Color4Extensions.FromHex(@"2299bb");
        public static readonly Color4 COLOUR_RIM_LEFT = Color4Extensions.FromHex(@"9b5de5");
        public static readonly Color4 COLOUR_RIM_RIGHT = Color4Extensions.FromHex(@"f28c28");

        public Hit()
        {
            TypeBindable.BindValueChanged(_ =>
            {
                updateSamplesFromType();
                updateDisplayColour();
            });

            SamplesBindable.BindCollectionChanged((_, _) => updateTypeFromSamples());
        }

        private void updateDisplayColour()
        {
            if (DisplayHand == null)
            {
                DisplayColour.Value = Type == HitType.Centre ? COLOUR_CENTRE : COLOUR_RIM;
                return;
            }

            DisplayColour.Value = (Type, DisplayHand) switch
            {
                (HitType.Centre, HitHand.Left) => COLOUR_CENTRE_LEFT,
                (HitType.Centre, HitHand.Right) => COLOUR_CENTRE_RIGHT,
                (HitType.Rim, HitHand.Left) => COLOUR_RIM_LEFT,
                (HitType.Rim, HitHand.Right) => COLOUR_RIM_RIGHT,
                _ => Type == HitType.Centre ? COLOUR_CENTRE : COLOUR_RIM,
            };
        }

        private void updateTypeFromSamples()
        {
            Type = getRimSamples().Any() ? HitType.Rim : HitType.Centre;
        }

        /// <summary>
        /// Returns an array of any samples which would cause this object to be a "rim" type hit.
        /// </summary>
        private HitSampleInfo[] getRimSamples() => Samples.Where(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE).ToArray();

        private void updateSamplesFromType()
        {
            var rimSamples = getRimSamples();

            bool isRimType = Type == HitType.Rim;

            if (isRimType != rimSamples.Any())
            {
                if (isRimType)
                    Samples.Add(CreateHitSampleInfo(HitSampleInfo.HIT_CLAP));
                else
                {
                    foreach (var sample in rimSamples)
                        Samples.Remove(sample);
                }
            }
        }

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        public class StrongNestedHit : StrongNestedHitObject
        {
            public StrongNestedHit(TaikoHitObject parent)
                : base(parent)
            {
            }
        }
    }

    public enum HitHand
    {
        Left,
        Right,
    }
}
