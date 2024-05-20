using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UtilEssentials.UIToolkitUtility.Editor.VisualElements
{
    public class FocusCompatableScrollView : ScrollView
    {

        #region UXML Factory
        [Preserve]
        public new class UxmlFactory : UxmlFactory<FocusCompatableScrollView, UxmlTraits> { }
        public FocusCompatableScrollView()
        {
            Init();
        }
        #endregion

        VisualElement _currentlyFocused;
        NavigationMoveEvent.Direction _navDirection;

        void Init()
        {
            RegisterCallback<NavigationMoveEvent>(evt =>
            {
                _currentlyFocused = evt.target as VisualElement;
                _navDirection = evt.direction;

                if (_currentlyFocused == null)
                {
                    return;
                }

                Camera main = Camera.main;
                if (main == null)
                {
                    return;
                }

                Vector2 topLeftPosition = this.LocalToWorld(this.contentRect.min);
                Vector2 bottomRightPosition = this.LocalToWorld(this.contentRect.max);
                Vector2 focusedPosition = _currentlyFocused.LocalToWorld(_currentlyFocused.contentRect.center);

                if (evt.direction == NavigationMoveEvent.Direction.Down && bottomRightPosition.y - keyboadEdgeDistance < focusedPosition.y)
                {
                    scrollOffset += Vector2.up * keyboadEdgeScrollAmount;
                }
                else if (evt.direction == NavigationMoveEvent.Direction.Up && topLeftPosition.y + keyboadEdgeDistance > focusedPosition.y)
                {
                    scrollOffset -= Vector2.up * keyboadEdgeScrollAmount;
                }


                Vector2 screenExtents = main.WorldToScreenPoint(new Vector2(Screen.currentResolution.width, Screen.currentResolution.height));
                //Debug.Log(new Vector2(main.pixelWidth, main.pixelHeight));
                //Debug.Log(_currentlyFocused.LocalToWorld(_currentlyFocused.contentRect.center));
                //Debug.Log(worldBound.center);

                //horizontalPageSize = screenExtents.x;
            });

            RegisterCallback<FocusOutEvent>(evt =>
            {
                var current = evt.target as VisualElement;
                var next = evt.relatedTarget as VisualElement;

                if (current == null)
                {
                    return;
                }

                if (next == null)
                {
                    return;
                }

                Vector2 currentPos = current.LocalToWorld(current.contentRect.center);
                Vector2 nextPos = next.LocalToWorld(next.contentRect.center);

                Action switchFocusLambda = () =>
                {
                    current.Focus();
                    _navDirection = NavigationMoveEvent.Direction.None;
                };

                switch (_navDirection)
                {
                    case NavigationMoveEvent.Direction.Up:
                        if (currentPos.y < nextPos.y)
                        {
                            CoroutineHost.instance.EndOfFrameAction(switchFocusLambda);
                        }
                        break;
                    case NavigationMoveEvent.Direction.Down:
                        if (currentPos.y > nextPos.y)
                        {
                            CoroutineHost.instance.EndOfFrameAction(switchFocusLambda);
                        }
                        break;
                    case NavigationMoveEvent.Direction.Left:
                        break;
                    case NavigationMoveEvent.Direction.Right:
                        break;
                    default:
                        break;
                }

            });
        }

        //void IsFirstElementFocused()
        //{

        //}

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlFloatAttributeDescription KeyboadEdgeDistance = new UxmlFloatAttributeDescription { name = "Keyboad-Edge-Distance", defaultValue = 200 };
            public UxmlFloatAttributeDescription KeyboadEdgeScrollAmount = new UxmlFloatAttributeDescription { name = "Keyboad-Edge-Scroll-Amount", defaultValue = 400 };
            public UxmlFloatAttributeDescription MouseWheelScrollSize = new UxmlFloatAttributeDescription { name = "Mouse-Wheel-Scroll-Size", defaultValue = 100 };

            //public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            //{
            //    get { yield break; }
            //}

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var ate = ve as FocusCompatableScrollView;

                base.Init(ve, bag, cc);
                ate.keyboadEdgeDistance = KeyboadEdgeDistance.GetValueFromBag(bag, cc);
                ate.keyboadEdgeScrollAmount = KeyboadEdgeScrollAmount.GetValueFromBag(bag, cc);
                ate.mouseWheelScrollSize = MouseWheelScrollSize.GetValueFromBag(bag, cc);
            }
        }

        public float keyboadEdgeDistance
        {
            get; set;
        }
        public float keyboadEdgeScrollAmount
        {
            get; set;
        }
        public float mouseWheelScrollSizeDummy
        {
            get => mouseWheelScrollSize;
            set => mouseWheelScrollSize = value;
        }
    }
}
