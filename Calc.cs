// ATTRIB: HideScenery
using System;
using LinkedMovement.Selection;
using LinkedMovement.Utils;
using UnityEngine;
using static LinkedMovement.Selection.Visibility;
using static LinkedMovement.Selection.SelectionAction;
using Op = LinkedMovement.Selection.SelectionOperation;
using Handler = LinkedMovement.SelectionHandler;

namespace LinkedMovement {
    internal sealed class Calc {
        private readonly Park park;
        private readonly Handler handler;
        private Options Options => handler.Options;

        public Calc(Handler handler) {
            this.handler = handler;
            Debug.Assert(GameController.Instance != null);
            park = GameController.Instance.park;
            Debug.Assert(park != null);
        }

        private bool IsHidden(BuildableObject o) => handler.IsHidden(o);
        private static bool IsPath(BuildableObject o) => o is Path;
        private static bool IsDeco(BuildableObject o) => o is Deco;
        private static bool IsAttraction(BuildableObject o) => o is Attraction;
        private Visibility CalcVisibilityWhenNeitherPathNorDeco(BuildableObject o) {
            Debug.Assert(!(o is Path));
            Debug.Assert(!(o is Deco));
            if (park.hideAttractionsEnabled && IsAttraction(o)) {
                return HiddenByParkitect;
            }
            else {
                return Visibility.Block;
            }
        }
        public Visibility BuildableObjectVisibility(BuildableObject o) {
            if (o.isPreview) {
                return Ignore;
            }

            var isPath = IsPath(o);
            var isDeco = IsDeco(o);

            if (!(isPath || isDeco)) {
                return CalcVisibilityWhenNeitherPathNorDeco(o);
            }

            if (isPath && park.hidePathsEnabled) {
                LinkedMovement.Log("HidePathEnabled on Paths");
                return HiddenByParkitect;
            }
            if (isDeco && park.hideSceneryEnabled) {
                LinkedMovement.Log("HideSceneryEnabled on Deco");
                return HiddenByParkitect;
            }

            return IsHidden(o) ? Hidden : Visible;
        }

        public SelectionAction BoxAction(SelectionOperation op, Bounds bounds, BuildableObject o) {
            return Action(op, bounds, o, handler.Options.BoxOptions);
        }

        public SelectionAction Action(SelectionOperation op, Bounds bounds, BuildableObject o, AdvancedOptions options) {
            if (o.isPreview) {
                return DoNothing;
            }

            if (IsPath(o)) {
                return PathAction(op, bounds, o, options);
            }
            else if (IsDeco(o)) {
                return DecoAction(op, bounds, o, options);
            }
            else {
                return DoNothing;
            }
        }

        private SelectionAction PathAction(SelectionOperation op, Bounds bounds, BuildableObject o, AdvancedOptions options) {
            Debug.Assert(IsPath(o));

            if (park.hidePathsEnabled) {
                return DoNothing;
            }

            return op switch {
                Op.Remove when options.ApplyFiltersOnAddOnly => IsHidden(o) ? Remove : DoNothing,
                var _ when !options.HidePaths => DoNothing,
                _ => CalcAddRemove(op, o),
            };
        }
        private SelectionAction DecoAction(SelectionOperation op, Bounds bounds, BuildableObject o, AdvancedOptions options) {
            Debug.Assert(IsDeco(o));

            if (park.hideSceneryEnabled) {
                return DoNothing;
            }

            switch (op) {
                case Op.Remove when options.ApplyFiltersOnAddOnly:
                    return IsHidden(o) ? Remove : DoNothing;
                case var _ when !options.HideScenery:
                    return DoNothing;
            }

            var sceneryType = CalcSceneryType(o);
            if (!options.SceneryToHide.HasSet(sceneryType)) {
                return DoNothing;
            }

            if (RequiresCompletelyInBounds(sceneryType, options)) {
                var boundsOptions = sceneryType switch {
                    SceneryType.Wall => options.WallOptions.BoundsOptions,
                    SceneryType.Roof => options.RoofOptions.BoundsOptions,
                    _ => options.OtherSceneryOptions.BoundsOptions,
                };
                if (!IsCompletelyInBounds(o, bounds, new(options.BoundsOptions, boundsOptions))) {
                    return DoNothing;
                }
            }

            return sceneryType switch {
                SceneryType.Wall => WallAction(op, bounds, o, options.WallOptions),
                _ => CalcAddRemove(op, o),
            };
        }
        private bool RequiresCompletelyInBounds(SceneryType sceneryType, AdvancedOptions options)
          => sceneryType switch {
              SceneryType.Wall => options.WallOptions.BoundsOptions.OnlyMatchCompletelyInBounds.Or(options.BoundsOptions.OnlyMatchCompletelyInBounds),
              SceneryType.Roof => options.RoofOptions.BoundsOptions.OnlyMatchCompletelyInBounds.Or(options.BoundsOptions.OnlyMatchCompletelyInBounds),
              SceneryType.Other => options.OtherSceneryOptions.BoundsOptions.OnlyMatchCompletelyInBounds.Or(options.BoundsOptions.OnlyMatchCompletelyInBounds),
              _ => options.BoundsOptions.OnlyMatchCompletelyInBounds,
          };
        private SelectionAction WallAction(SelectionOperation op, Bounds bounds, BuildableObject o, WallOptions options) {
            Debug.Assert(CalcSceneryType(o) == SceneryType.Wall);

            // bounds already checked in `DecoAction`
            if (options.HideOnlyFacingCurrentView) {
                switch (o) {
                    case Wall wall: {
                            var sidesToHide = BlockSideHelper.CalcFrontSidesFromCurrentView();
                            var side = wall.getBuiltOnSide();

                            if (sidesToHide.HasSide(side)) {
                                return CalcAddRemove(op, o);
                            }
                            else {
                                if (options.UpdateNotFacingCurrentView) {
                                    return CalcRemove(o);
                                }
                                else {
                                    return DoNothing;
                                }
                            }
                        }
                    default:
                        return CalcAddRemove(op, o);
                }
            }
            else {
                return CalcAddRemove(op, o);
            }
        }

        public SelectionAction HideSceneryInBoundsAction(SelectionOperation op, Bounds bounds, BuildableObject o) {
            return Action(op, bounds, o, handler.Options.HideInBoundsOptions);
        }

        private bool IsCompletelyInBounds(BuildableObject o, Bounds bounds, ResolvedBoundsOptions boundsOptions) {
            foreach (var coll in o.mouseColliders) {
                var collBounds = coll.oldBounds;
                Debug.Assert(collBounds != new Bounds());
                switch (boundsOptions.Precision) {
                    case Precision.Exact when !bounds.ContainsCompletely(collBounds):
                        return false;
                    case Precision.Approximately when !bounds.ContainsCompletelyApproximately(collBounds, boundsOptions.Epsilon):
                        return false;
                }
            }
            return true;
        }

        private SelectionAction CalcAdd(BuildableObject o) => IsHidden(o) ? DoNothing : Add;
        private SelectionAction CalcRemove(BuildableObject o) => IsHidden(o) ? Remove : DoNothing;
        private SelectionAction CalcAddRemove(SelectionOperation op, BuildableObject o) {
            return op switch {
                Op.Add => CalcAdd(o),
                Op.Remove => CalcRemove(o),
                _ => throw new InvalidOperationException("Unknown Operation " + op),
            };
        }
        private bool IsCategory(BuildableObject o, string category)
          => o.getCategoryTag() == category;
        private bool NameContains(BuildableObject o, string str)
          => o.getUnlocalizedName().Contains(str, StringComparison.InvariantCultureIgnoreCase);
        private bool IsWall(BuildableObject o) {
            var hideWallsBy = handler.Options.BoxOptions.WallOptions.HideBy;
            return
                (hideWallsBy.HasSet(HideType.Class) && o is Wall && !(o is Fence))
              ||
                (hideWallsBy.HasSet(HideType.Category) && IsCategory(o, "Structures/Walls"))
              ||
                (hideWallsBy.HasSet(HideType.Name) && NameContains(o, "wall"))
              ;
        }
        private bool IsRoof(BuildableObject o) {
            var hideRoofsBy = handler.Options.BoxOptions.RoofOptions.HideBy;
            return
                (hideRoofsBy.HasSet(HideType.Category) && IsCategory(o, "Structures/Roofs"))
              ||
                (hideRoofsBy.HasSet(HideType.Name) && NameContains(o, "roof"))
              ;
        }
        private SceneryType CalcSceneryType(BuildableObject o) {
            if (IsWall(o)) {
                return SceneryType.Wall;
            }
            if (IsRoof(o)) {
                return SceneryType.Roof;
            }
            return SceneryType.Other;
        }
    }
}
