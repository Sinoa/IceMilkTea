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
using System.Linq;

namespace IceMilkTea.Video
{
    public class ImtVideoPlayer : MonoBehaviour, IImtVideoPlayer
    {
        private List<ImtVideoTimeMarker> markerList;
        private List<IImtVideoPlayerEventListener> eventListenerList;
        private VideoPlayer unityVideoPlayer;
        private Queue<ImtVideoTimeMarker> markerQueue;



        public RenderTexture RenderTarget => unityVideoPlayer.targetTexture;



        public static ImtVideoPlayer Create(VideoClip videoClip)
        {
            return Create(videoClip, new RenderTexture((int)videoClip.width, (int)videoClip.height, 0, RenderTextureFormat.BGRA32, 0));
        }


        public static ImtVideoPlayer Create(VideoClip videoClip, RenderTexture outsideRenderTexture)
        {
            var gameObject = GetOrCreateVideoPlayerGameObject();
            var videoPlayer = CreateVideoPlayerComponent(gameObject);
            videoPlayer.unityVideoPlayer.clip = videoClip;
            videoPlayer.unityVideoPlayer.targetTexture = outsideRenderTexture;


            return videoPlayer;
        }


        private static ImtVideoPlayer CreateVideoPlayerComponent(GameObject gameObject)
        {
            var videoPlayer = gameObject.AddComponent<VideoPlayer>();
            var imtVideoPlayer = gameObject.AddComponent<ImtVideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.isLooping = false;
            videoPlayer.aspectRatio = VideoAspectRatio.FitVertically;


            imtVideoPlayer.unityVideoPlayer = videoPlayer;
            imtVideoPlayer.markerList = new List<ImtVideoTimeMarker>();
            imtVideoPlayer.eventListenerList = new List<IImtVideoPlayerEventListener>();
            imtVideoPlayer.markerQueue = new Queue<ImtVideoTimeMarker>();
            return imtVideoPlayer;
        }


        private static GameObject GetOrCreateVideoPlayerGameObject()
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
            if (unityVideoPlayer.isPlaying) return;


            if (!unityVideoPlayer.isPaused)
            {
                markerQueue.Clear();
                foreach (var marker in markerList.OrderBy(x => x.MarkedTime))
                {
                    markerQueue.Enqueue(marker);
                }
            }


            unityVideoPlayer.Play();
            foreach (var listener in eventListenerList)
            {
                listener.OnPlayVideo(this);
            }
        }


        public void Pause()
        {
            if (unityVideoPlayer.isPaused) return;


            unityVideoPlayer.Pause();
            foreach (var listener in eventListenerList)
            {
                listener.OnPauseVideo(this);
            }
        }


        public void Stop()
        {
            if (!unityVideoPlayer.isPlaying && !unityVideoPlayer.isPaused)
            {
                return;
            }


            unityVideoPlayer.Stop();
            foreach (var listener in eventListenerList)
            {
                listener.OnStopVideo(this);
            }
        }


        public void AddMarker(int id, float time, object userObject)
        {
            AddMarker(new ImtVideoTimeMarker(id, time, userObject));
        }


        public void AddMarker(ImtVideoTimeMarker marker)
        {
            markerList.Add(marker);
        }


        public void AddEventListener(IImtVideoPlayerEventListener listener)
        {
            if (eventListenerList.Contains(listener)) return;
            eventListenerList.Add(listener);
        }


        public void RemoveEventListener(IImtVideoPlayerEventListener listener)
        {
            eventListenerList.Remove(listener);
        }


        private void RaiseMarkerEvent(ImtVideoTimeMarker marker)
        {
            foreach (var listener in eventListenerList)
            {
                listener.OnVideoTimeMarkerTriggered(this, marker);
            }
        }


        private void Update()
        {
            if (markerQueue.Count == 0) return;


            var marker = markerQueue.Dequeue();
            while (marker != null && unityVideoPlayer.time >= marker.MarkedTime)
            {
                RaiseMarkerEvent(marker);
                if (markerQueue.Count == 0)
                {
                    break;
                }


                marker = markerQueue.Dequeue();
            }
        }
    }
}