using System;
using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UtilEssentials.UIToolkitUtility;

namespace UtilEssentials.InputIconBinding
{


    /// <summary>
    /// The data contained within an icon bind including information on the texture and animation
    /// </summary>
    [Serializable]
    internal struct IconBindData
    {
        [SerializeField]
        IconType _type;
        IconType type => _type;

        [SerializeField]
        Texture2D _texture;
        public Texture2D texture => _texture;

        [SerializeField]
        Sprite[] _animSprites;

        [SerializeField]
        float _animationTime;

        /// <param name="ve">The visual element for this icon to be bound to</param>
        /// <returns>Whether the binding was successful</returns>
        public bool BindIconToVisualElement(VisualElement ve, out Action cancelAnimationDelegate)
        {
            cancelAnimationDelegate = null;
            if (_texture == null)
            {
                return false;
            }

            switch (_type)
            {
                case IconType.Static:
                    ve.style.backgroundImage = _texture;
                    return true;
                case IconType.Animated_ReverseLoop:
                    break;
                case IconType.Animated_RolloverLoop:
                    break;
                default:
                    return false;
            }
#if UNITY_EDITOR
            if (ve.panel != null && (ve.panel.contextType == ContextType.Editor))
            {
                foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                {
                    // Check if the VisualElement is contained within the root VisualElement of the EditorWindow
                    if (window.rootVisualElement.Contains(ve))
                    {
                        EditorCoroutine editorCoroutine = window.StartCoroutine(IconAnimation(ve
                            #if UNITY_EDITOR
                            , false
                            #endif
                            ));

                        cancelAnimationDelegate += () =>
                        {
                            if (ve == null || ve.parent == null)
                            {
                                return;
                            }

                            if (editorCoroutine == null || window == null)
                            {
                                return;
                            }

                            window.StopCoroutine(editorCoroutine);
                            ve.style.backgroundImage = null;

                        };


                        return true;
                    }
                }
                return false;
            }
#endif
            Coroutine coroutine = CoroutineHost.instance.StartCoroutine(IconAnimation(ve
                            #if UNITY_EDITOR
                            , true
                            #endif
                            ));

            cancelAnimationDelegate += () =>
            {
                if (ve == null || ve.parent == null)
                {
                    return;
                }

                if (coroutine == null)
                {
                    return;
                }

                CoroutineHost.instance.StopCoroutine(coroutine);
                ve.style.backgroundImage = null;

            };
            return true;
        }

        /// <param name="ve">Visual element that will have the animated icon</param>
        /// <param name="runtime">Only in the editor, refines whether this is an Editor Window Corouitine (false), or a runtime coroutine (true)</param>
        /// <returns></returns>
        IEnumerator IconAnimation(VisualElement ve
            #if UNITY_EDITOR
            , bool runtime
            #endif
            )

        {
            bool reversed = false;
            int length = _animSprites.Length;
            if (length == 0)
            {
                ve.style.backgroundImage = _texture;
                yield break;
            }
            int currentIndex = 0;
            float waitTime = 0;
#if UNITY_EDITOR
            double previousTotalTime = 0;
            if (!runtime)
            {
                previousTotalTime = EditorApplication.timeSinceStartup;
            }
#endif
            ve.style.backgroundImage = new StyleBackground(_animSprites[0]);

            // Delayed till the end of the frame to make sure the element gets parented to another
            yield return null;

            // Loop
            while (true)
            {
                // If this has been deleted or has no parent, end the loop
                if (ve == null || ve.parent == null)
                {
                    yield break;
                }

                // Advances to the next frame/s depending on the amount of time passed since the last coroutine tick
                int advanceFrame = 0;
                while (_animationTime / (float)length <= waitTime)
                {
                    advanceFrame++;
                    if (_animationTime <= 0)
                    {
                        waitTime = 0;
                        break;
                    }
                    waitTime -= _animationTime / (float)length;
                }
                for (int i = 0; i < advanceFrame; i++)
                {
                    currentIndex = reversed ? currentIndex - 1 : currentIndex + 1;
                    if (currentIndex == length || currentIndex < 0)
                    {
                        switch (_type)
                        {
                            case IconType.Animated_ReverseLoop:
                                reversed = !reversed;
                                currentIndex = reversed ? currentIndex - 1 : currentIndex + 1;
                                break;
                            case IconType.Animated_RolloverLoop:
                                currentIndex = 0;
                                break;
                            default:
                                break;
                        }
                    }
                }

                ve.style.backgroundImage = new StyleBackground(_animSprites[currentIndex]);

                yield return null;
#if UNITY_EDITOR
                if (!runtime)
                {
                    waitTime += (float)(EditorApplication.timeSinceStartup - previousTotalTime);
                    previousTotalTime = EditorApplication.timeSinceStartup;
                }
                else
                {
#endif
                    waitTime += Time.deltaTime;
#if UNITY_EDITOR
                }
#endif
            }
        }
    }
}