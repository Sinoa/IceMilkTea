// zlib/libpng License
//
// Copyright (c) 2020 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using UnityEngine;

namespace IceMilkTea.UI
{
    public class TextCountupPrintAnimator : ImtTextAnimator
    {
        [SerializeField]
        private int CharaPerMiliSecond = 20;

        private float charaPerSecond;
        private float startTime;
        private int currentDecorationEntryIndex;
        private int shownRubyTextIndex;



        protected virtual void Update()
        {
            if (TargetRubyableText == null)
            {
                return;
            }


            charaPerSecond = CharaPerMiliSecond / 1000.0f;
            TargetRubyableText.SetVerticesDirty();
        }


        public override void OnTextChanged()
        {
            startTime = Time.time;
            enabled = true;
            currentDecorationEntryIndex = 0;
            shownRubyTextIndex = 0;


            if (TargetRubyableText.DecorationEntryCount == 0)
            {
                return;
            }
        }


        private int GetShownCharaCount()
        {
            var elapseTime = Time.time - startTime;
            return (int)(elapseTime / charaPerSecond);
        }


        public override void AnimateMainCharaVertex(UIVertex[] charVertexces, int textIndex)
        {
            var shownCharaCount = GetShownCharaCount();
            if (TargetRubyableText.MainText.Length < shownCharaCount)
            {
                enabled = false;
                shownCharaCount = TargetRubyableText.MainText.Length;
            }


            if (textIndex <= shownCharaCount)
            {
                return;
            }


            for (int i = 0; i < charVertexces.Length; ++i)
            {
                var vertex = charVertexces[i];
                vertex.position = Vector3.zero;
                charVertexces[i] = vertex;
            }
        }


        public override void AnimateRubyCharaVertex(UIVertex[] charVertexces, int textIndex)
        {
            if (TargetRubyableText.DecorationEntryCount == 0)
            {
                return;
            }


            TargetRubyableText.TryGetDecorationEntry(currentDecorationEntryIndex, out var entry);
            var shownMainCharaCount = GetShownCharaCount();
            if (shownMainCharaCount >= (entry.MainTextStartIndex + entry.MainTextCharacterCount - 1))
            {
                shownRubyTextIndex = entry.RubyTextStartIndex + entry.RubyTextCharacterCount;
                if ((TargetRubyableText.DecorationEntryCount - 1) > currentDecorationEntryIndex)
                {
                    ++currentDecorationEntryIndex;
                }
            }


            if (textIndex < shownRubyTextIndex)
            {
                return;
            }


            for (int i = 0; i < charVertexces.Length; ++i)
            {
                var vertex = charVertexces[i];
                vertex.position = Vector3.zero;
                charVertexces[i] = vertex;
            }
        }
    }
}
