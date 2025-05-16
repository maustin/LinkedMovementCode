// ATTRIB: HideScenery
using System;
using System.Runtime.CompilerServices;
using LinkedMovement.Selection;

// TODO: Look at eliminating this
namespace LinkedMovement {
    internal sealed class Options {
        public float Transparency {
            get => Settings.Instance.seeThroughObjectsAlpha;
            set {
                if (value != Transparency) {
                    Settings.Instance.seeThroughObjectsAlpha = value;
                    OnPropertyChanged();
                }
            }
        }

        private Mode mode = Mode.None;
        public Mode Mode {
            get => mode;
            set {
                if (value != mode) {
                    mode = value;
                    OnPropertyChanged();
                }
            }
        }

        public readonly BoxOptions BoxOptions = new();

        public readonly HideInBoundsOptions HideInBoundsOptions = new();

        public delegate void PropertyChanged(Options options, string property);
        public event PropertyChanged Changed;
        private void OnPropertyChanged([CallerMemberName] string property = "") => Changed?.Invoke(this, property);
    }
    [Flags]
    internal enum SceneryType {
        Roof = 1 << 0,
        Wall = 1 << 1,
        Other = 1 << 2,
        All = Roof | Wall | Other,
    }
    [Flags]
    internal enum HideType {
        Name = 1 << 0,
        Category = 1 << 1,
        Class = 1 << 2,
        All = Name | Category | Class,
    }
    internal static class TypeExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSet(this SceneryType value, SceneryType flag)
          => (value & flag) == flag;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSet(this HideType value, HideType flag)
          => (value & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SceneryType Add(this SceneryType value, SceneryType flag)
          => value | flag;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HideType Add(this HideType value, HideType flag)
          => value | flag;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SceneryType Remove(this SceneryType value, SceneryType flag)
          => value & ~flag;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HideType Remove(this HideType value, HideType flag)
          => value & ~flag;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SceneryType Set(this SceneryType value, SceneryType flag, bool enabled)
          => enabled ? value | flag : value & ~flag;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HideType Set(this HideType value, HideType flag, bool enabled)
          => enabled ? value | flag : value & ~flag;
    }

    internal abstract class AdvancedOptions {
        public bool ApplyFiltersOnAddOnly = true;
        public BoundsOptions BoundsOptions = new() {
            OnlyMatchCompletelyInBounds = true,
            Precision = Precision.Approximately,
            Epsilon = 0.05f,
        };

        public bool HidePaths = true;
        public bool HideScenery = true;

        public SceneryType SceneryToHide = SceneryType.All;
        public RoofOptions RoofOptions = new() {
            HideBy = HideType.All,
            BoundsOptions = new(),
        };
        public WallOptions WallOptions = new() {
            HideBy = HideType.All,
            BoundsOptions = new(),
            HideOnlyFacingCurrentView = false,
            UpdateNotFacingCurrentView = true,
        };

        public OtherSceneryOptions OtherSceneryOptions = new() {
            BoundsOptions = new(),
        };
    }

    internal sealed class BoxOptions : AdvancedOptions { }

    internal sealed class RoofOptions {
        public HideType HideBy = HideType.All;  // cannot be class
        public MaybeInheritedBoundsOptions BoundsOptions = new();
    }
    internal sealed class WallOptions {
        public HideType HideBy = HideType.All;
        public MaybeInheritedBoundsOptions BoundsOptions = new();
        public bool HideOnlyFacingCurrentView = false;
        public bool UpdateNotFacingCurrentView = true;
    }
    internal sealed class OtherSceneryOptions {
        public MaybeInheritedBoundsOptions BoundsOptions = new();
    }

    internal sealed class HideInBoundsOptions : AdvancedOptions {
        public float Height = 4.0f;
    }

    internal sealed class BoundsOptions {
        public bool OnlyMatchCompletelyInBounds = true;
        public Precision Precision = Precision.Approximately;
        public float Epsilon = 0.05f;
        //todo: limit direction (x,y,both)? -> disable x for box selection?
    }
    internal sealed class MaybeInheritedBoundsOptions {
        public MaybeInherit<bool> OnlyMatchCompletelyInBounds = default;
        public MaybeInherit<Precision> Precision = default;
        public MaybeInherit<float> Epsilon = default;

        public bool AllInherited
          =>
            OnlyMatchCompletelyInBounds.IsInherited
            && Precision.IsInherited
            && Epsilon.IsInherited
          ;
    }
    internal struct ResolvedBoundsOptions {
        readonly BoundsOptions Root;
        readonly MaybeInheritedBoundsOptions MaybeInherited;
        //todo: allow chaining? root -> maybe inherited -> maybe inherited -> ... -> maybe inherited
        public ResolvedBoundsOptions(BoundsOptions root, MaybeInheritedBoundsOptions maybeInherited) {
            Root = root;
            MaybeInherited = maybeInherited;
        }

        public bool OnlyMatchCompletelyInBounds => MaybeInherited.OnlyMatchCompletelyInBounds.Or(Root.OnlyMatchCompletelyInBounds);
        public Precision Precision => MaybeInherited.Precision.Or(Root.Precision);
        public float Epsilon => MaybeInherited.Epsilon.Or(Root.Epsilon);
    }

    /*
      type MaybeInherit =
        | Value of 'T
        | Inherit
    */
    internal readonly struct MaybeInherit<T> {
        // hasValue instead of inherit: default (false) is inherit
        private readonly bool hasValue;
        private readonly T value;

        public MaybeInherit(T value) {
            hasValue = true;
            this.value = value;
        }
        public MaybeInherit() {
            hasValue = false;
            value = default;
        }

        public bool IsInherited => !hasValue;
        public bool HasValue => hasValue;
        public T ForceValue => value;
    }
    internal static class MaybeInherit {
        internal static MaybeInherit<T> Inherit<T>() => new();
        internal static MaybeInherit<T> Value<T>(T value) => new(value);
        internal static bool TryGetValue<T>(this in MaybeInherit<T> mi, out T value) {
            if (mi.IsInherited) {
                value = default;
                return false;
            }
            else {
                value = mi.ForceValue;
                return true;
            }
        }

        internal static T Or<T>(this in MaybeInherit<T> mi, T root)
          => mi.IsInherited ? root : mi.ForceValue;
    }

    internal enum Precision {
        Exact,
        Approximately,
    }
}
