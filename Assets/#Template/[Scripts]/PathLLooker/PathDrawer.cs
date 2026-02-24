using System;
using System.Collections.Generic;
using DancingLineFanmade.Guideline;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace DancingLineFanmade.PathPrediction
{
    [ExecuteInEditMode]
    public class PathDrawer : MonoBehaviour
    {
        [OnValueChanged(nameof(RefreshTaps))]
        public Transform GuidelineTapHolder;
        public Color lineColor = Color.green;
        public Color boxColor = Color.red;
        
        [OnValueChanged(nameof(UpdateStyle))] 
        public Color textColor = Color.white;
        
        [OnValueChanged(nameof(ValidateFormat))]
        public string timeFormat = "0.00s";

        [ReadOnly, SerializeField] List<(Transform transform, GuidelineTap guidelineTap)> _taps = new();

        private GUIStyle _labelStyle;
        private Texture2D _backgroundTexture;

        private void OnEnable()
        {
            UpdateStyle();
            RefreshTaps();
        }

        private void OnDisable()
        {

            if (_backgroundTexture != null) DestroyImmediate(_backgroundTexture);
        }

        private void RefreshTaps()
        {
            if (GuidelineTapHolder == null) return;
            _taps.Clear();

            for (int i = 0; i < GuidelineTapHolder.childCount; i++)
            {
                Transform tap = GuidelineTapHolder.GetChild(i);
                if (tap.TryGetComponent<GuidelineTap>(out var tapComponent))
                {
                    _taps.Add((tap, tapComponent));
                }
            }
        }

        private void UpdateStyle()
        {
            if (_labelStyle == null) _labelStyle = new GUIStyle();
            
            if (_backgroundTexture == null)
            {
                _backgroundTexture = new Texture2D(1, 1);
                _backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            Color bgColor = new (1 - textColor.r, 1 - textColor.g, 1 - textColor.b, 0.91f);
            _backgroundTexture.SetPixel(0, 0, bgColor);
            _backgroundTexture.Apply();

            _labelStyle.normal.textColor = textColor;
            _labelStyle.normal.background = _backgroundTexture;
            _labelStyle.padding = new RectOffset(5, 5, 2, 2);
            _labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void ValidateFormat()
        {
            try { 0f.ToString(timeFormat); }
            catch { timeFormat = "0.00s"; }
        }

        void OnDrawGizmos()
        {
            if (_taps == null || _taps.Count == 0) return;
            if (_labelStyle == null || _labelStyle.normal.background == null) UpdateStyle();

            for (int i = 0; i < _taps.Count; i++)
            {
                var current = _taps[i];
                if (current.transform == null || current.guidelineTap == null) continue;

                if (i < _taps.Count - 1)
                {
                    var next = _taps[i + 1];
                    if (next.transform != null)
                    {
                        Gizmos.color = lineColor;
                        Gizmos.DrawLine(current.transform.position, next.transform.position);
                    }
                }
                string text = current.guidelineTap.triggerTime.ToString(timeFormat);
                Handles.Label(current.transform.position + Vector3.up * 0.8f, text, _labelStyle);

                Gizmos.color = boxColor;
                Gizmos.DrawCube(current.transform.position, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }
    }
}