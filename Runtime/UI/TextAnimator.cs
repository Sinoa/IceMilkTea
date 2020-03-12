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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IceMilkTea.UI
{
    public class TextAnimator : UIBehaviour, IMeshModifier
    {
        public new void OnValidate()
        {
            base.OnValidate();

            var graphics = base.GetComponent<Graphic>();
            if (graphics != null)
            {
                graphics.SetVerticesDirty();
            }
        }

        public void ModifyMesh(Mesh mesh) { }
        public void ModifyMesh(VertexHelper verts)
        {
            List<UIVertex> stream = new List<UIVertex>();
            verts.GetUIVertexStream(stream);

            modify(ref stream);

            verts.Clear();
            verts.AddUIVertexTriangleStream(stream);
        }

        void modify(ref List<UIVertex> stream)
        {
        }
    }
}
