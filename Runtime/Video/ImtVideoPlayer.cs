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
using UnityEngine.Video;

namespace IceMilkTea.Video
{
    public class ImtVideoPlayer : MonoBehaviour, IImtVideoPlayer
    {
        private List<ImtVideoTimeMarker> markerList;
        private List<IImtVideoPlayerEventListener> eventListenerList;
        private VideoPlayer unityVideoPlayer;
        private RenderTexture renderTarget;



        public RenderTexture RenderTarget => renderTarget;



        public static ImtVideoPlayer Create(VideoClip videoClip)
        {
            throw new NotImplementedException();
        }


        public static ImtVideoPlayer Create(VideoClip videoClip, RenderTexture outsideRenderTexture)
        {
            throw new NotImplementedException();
        }


        public static ImtVideoPlayer Create(VideoClip videoClip, Camera directRenderTargetCamera)
        {
            throw new NotImplementedException();
        }


        private GameObject GetOrCreateVideoPlayerGameObject()
        {
            var gameObjectName = "__IMT_VIDEOPLAYER_GAMEOBJECT__";
            var gameObject = GameObject.Find(gameObjectName);
            if (gameObject == null)
            {
                gameObject = new GameObject(gameObjectName);
                DontDestroyOnLoad(gameObject);


                var transform = gameObject.GetComponent<Transform>();
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }


            return gameObject;
        }


        public void Play()
        {
        }


        public void Pause()
        {
        }


        public void Stop()
        {
        }


        public void AddMarker(float time, object userObject)
        {
            AddMarker(new ImtVideoTimeMarker(time, userObject));
        }


        public void AddMarker(ImtVideoTimeMarker marker)
        {
            throw new NotImplementedException();
        }


        public void AddEventListener(IImtVideoPlayerEventListener listener)
        {
        }


        public void RemoveEventListener(IImtVideoPlayerEventListener listener)
        {
        }
    }
}