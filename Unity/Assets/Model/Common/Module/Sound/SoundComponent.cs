using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace ETModel
{
    public class AudioSourceComponent : Component
    {
        public object key=null;
        public AudioSource audioSource=null;
        public AudioSourceComponent next=null;
        public AudioSourceComponent previous=null;
        public bool isFade=false;
        public bool isSilenceStop=false;
    }

    public class SoundComponent : Component
    {
        public static SoundComponent Instance;

        public List<AudioSourceComponent> items=new List<AudioSourceComponent>();
        public Queue<AudioSourceComponent> waitItems = new Queue<AudioSourceComponent>();
        public Dictionary<object, AudioSourceComponent> itemDict=new Dictionary<object, AudioSourceComponent>();

        public GameObject rootGameObject;
        public bool     isSilence=false;
        public float    volume=1f;
        public int      refreshFrame=0;
    }

    [ObjectSystem]
    public class SoundComponentAwakeSystem : AwakeSystem<SoundComponent, GameObject>
    {
        public override void Awake(SoundComponent self,GameObject rootGameObject)
        {
            self.rootGameObject = rootGameObject;
        }
    }

    [ObjectSystem]
    public class SoundComponentUpdateSystem : UpdateSystem<SoundComponent>
    {
        public override void Update(SoundComponent self)
        {
            self.Update();
        }
    }

    [ObjectSystem]
    public class SoundComponentDestroySystem : DestroySystem<SoundComponent>
    {
        public override void Destroy(SoundComponent self)
        {
            self.Destroy();
        }
    }


    public static class SoundComponentSystem
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        public static void Destroy(this SoundComponent self)
        {
            foreach (AudioSourceComponent _item in self.items)
            {
                _item.audioSource.clip = null;
                GameObject.Destroy(_item.audioSource);

                _item.audioSource = null;
                _item.next = null;
                _item.previous = null;
                _item.key = null;
            }

            self.waitItems.Clear();
            self.items.Clear();
            self.itemDict.Clear();
        }

        /// <summary>
        /// 刷新播放队列
        /// </summary>
        public static void Update(this SoundComponent self)
        {
            if (self.refreshFrame != UnityEngine.Time.frameCount)
            {
                self.refreshFrame = UnityEngine.Time.frameCount;

                AudioSourceComponent _item = null;

                for (int i = 0; i < self.items.Count; i++)
                {
                    _item = self.items[i];

                    if (_item.audioSource.enabled && !_item.audioSource.isPlaying)
                    {
                        if (_item.key == null)
                        {
                            //归还
                            _item.audioSource.clip = null;
                            _item.audioSource.enabled = false;
                            self.waitItems.Enqueue(_item);
                        }
                        else
                        {
                            if (_item.previous == null) //如果是第一个
                            {
                                if (_item.next != null) //后面有兄弟节点，链表中移除自己
                                {
                                    //变更双向链表
                                    _item.next.previous = null;
                                    //更新站位元素
                                    self.itemDict[_item.key] = _item.next;

                                    _item.next = null;
                                    _item.key = null;

                                    //归还
                                    _item.audioSource.clip = null;
                                    _item.audioSource.enabled = false;
                                    self.waitItems.Enqueue(_item);
                                }
                                else //后面没有了直接移除链表
                                {
                                    //移除站位元素
                                    self.itemDict.Remove(_item.key);

                                    _item.key = null;

                                    //归还
                                    _item.audioSource.clip = null;
                                    _item.audioSource.enabled = false;
                                    self.waitItems.Enqueue(_item);
                                }
                            }
                            else
                            {

                                if (_item.next != null) //后面有兄弟节点，链表中移除自己
                                {
                                    //变更双向链表
                                    _item.previous.next = _item.next;
                                    _item.next.previous = _item.previous;


                                    _item.previous = null;
                                    _item.next = null;
                                    _item.key = null;

                                    //归还
                                    _item.audioSource.clip = null;
                                    _item.audioSource.enabled = false;
                                    self.waitItems.Enqueue(_item);
                                }
                                else
                                {
                                    //变更双向链表
                                    _item.previous.next = null;


                                    _item.previous = null;
                                    _item.key = null;

                                    //归还
                                    _item.audioSource.clip = null;
                                    _item.audioSource.enabled = false;
                                    self.waitItems.Enqueue(_item);
                                }

                            }
                        }
                    }
                }

                //#if DEBUG_LOG
                //                Debug.Log("[" + this.GetHashCode().ToString() + "] Refresh() m_waitItems=" + m_waitItems.Count + "; m_items=" + m_items.Count + ";m_ItemsTagMap=" + m_itemTagMap.Count);
                //#endif
            }
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public static void SetVolume(this SoundComponent self,float value)
        {
            foreach (AudioSourceComponent _item in self.items)
            {
                _item.audioSource.volume = value;
            }
            self.volume = value;
        }

        /// <summary>
        /// 是否是静默状态
        /// </summary>
        public static void SetSilence(this SoundComponent self, bool value)
        {
            if (value)
            {
                if (self.isSilence) return;
                self.isSilence = true;
                foreach (AudioSourceComponent _item in self.items)
                {
                    _item.audioSource.volume = 0;
                }
            }
            else
            {
                if (self.isSilence)
                {
                    self.isSilence = false;
                    foreach (AudioSourceComponent _item in self.items)
                    {
                        _item.audioSource.volume = self.volume;
                    }
                }
            }
        }

        /// <summary>
        /// 获取item
        /// </summary>
        /// <returns></returns>
        private static AudioSourceComponent GetItem(this SoundComponent self)
        {
            AudioSourceComponent _item = null;

            self.Update();

            if (self.waitItems.Count > 0)
            {
                _item = self.waitItems.Dequeue();
                _item.audioSource.enabled = true;
            }
            else
            {
                //#if DEBUG_LOG

                //                Debug.Log("GetAudioSourceItem() New m_waitItems=" + m_waitItems.Count + "; m_totoalItem=" + m_items.Count + ";m_playingItems=" + m_itemTagMap.Count );
                //#endif
                AudioSource _audioSource = self.rootGameObject.AddComponent<AudioSource>();
                _item =ComponentFactory.Create<AudioSourceComponent>();
                _item.audioSource = _audioSource;
                self.items.Add(_item);
            }

            return _item;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void RecycleItem(this SoundComponent self, AudioSourceComponent item)
        {
            if (item == null) return;

            if (item.audioSource.isPlaying)
            {
                item.audioSource.Stop();
            }

            if (item.isFade)
            {
                item.audioSource.DOKill();
                item.isFade = false;
            }
            item.isSilenceStop = false;

            item.previous = null;
            item.next = null;
            item.key = null;

            //归还
            item.audioSource.clip = null;
            item.audioSource.enabled = false;
            self.waitItems.Enqueue(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toyDirName"></param>
        /// <param name="soundName"></param>
        /// <param name="loop"></param>
        /// <param name="duration"></param>
        /// <param name="key"></param>
        public static void Play(this SoundComponent self,string toyDirName,string soundName,bool loop = false, float duration = 0, object key = null)
        {
            self.Play(ResourcesComponent.Instance.LoadAsset($"{toyDirName}/Sounds/{soundName}",typeof(AudioClip)) as AudioClip, loop, duration, key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toyDirName"></param>
        /// <param name="soundName"></param>
        /// <param name="loop"></param>
        /// <param name="duration"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async ETVoid PlayAsync(this SoundComponent self, string toyDirName, string soundName, bool loop = false, float duration = 0, object key = null)
        {
            self.Play((await ResourcesComponent.Instance.LoadAssetAsync($"{toyDirName}/Sounds/{soundName}", typeof(AudioClip))) as AudioClip, loop, duration, key);
        }

        /// <summary>
        /// 播放一个声音
        /// </summary>
        /// <param name="audioClip">声音资源</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="key">声音标签，可将多个播任务对应到一个标签管理</param>
        public static void Play(this SoundComponent self, AudioClip audioClip, bool loop = false, float duration = 0, object key = null)
        {
            AudioSourceComponent _item = self.GetItem();
            _item.audioSource.clip = audioClip;
            _item.audioSource.loop = loop;

            if (key != null)
            {
                #region 有标签

                AudioSourceComponent _itemFirst;
                _item.key = key;

                if (self.itemDict.TryGetValue(key, out _itemFirst)) //加到头部，方便后面移除操作
                {
                    _item.next = _itemFirst;
                    _item.next.previous = _item;
                    self.itemDict[_item.key] = _item;
                }
                else
                {
                    self.itemDict.Add(_item.key, _item);
                }

                #endregion
            }

           
            _item.audioSource.Play();
            if (duration>0)
            {
                _item.audioSource.volume = 0;
                _item.audioSource.DOFade(self.volume, duration);
            }
            else
            {
                _item.audioSource.volume = self.volume;
            }
        }

        /// <summary>
        /// 切换标签对应的播放任务到新的声音，如果没有则等同于Play方法，标签有多个播放任务则只保留一个
        /// </summary>
        /// <param name="audioClip">声音资源</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="key">声音标签</param>
        public static void SwitchPlay(this SoundComponent self, AudioClip audioClip, bool loop,object key, float duration= 0)
        {
            if (key == null)
            {
                throw new Exception("覆盖播放 标签不能为空");
            }

            AudioSourceComponent _item = self.GetItem();
            _item.audioSource.clip = audioClip;
            _item.audioSource.loop = loop;
            _item.key = key;

            if (self.itemDict.TryGetValue(key, out AudioSourceComponent _itemFirst)) //加到头部，方便后面移除操作
            {
                AudioSourceComponent _itemNext = null;
                if (duration == 0)
                {
                    //旧声音全部消除
                    while (_itemFirst != null)
                    {
                        _itemNext = _itemFirst.next;
                        self.RecycleItem(_itemFirst);
                        _itemFirst = _itemNext;
                    }
                }
                else
                {
                    //旧声音进入渐隐
                    while (_itemFirst != null)
                    {
                        _itemNext = _itemFirst.next;
                        if (_itemFirst.isFade) _itemFirst.audioSource.DOKill();
                        _itemFirst.audioSource.DOFade(0, duration);
                        _itemFirst.isFade = true;
                        _itemFirst.isSilenceStop = true;
                        _itemFirst = _itemNext;
                    }

                }
            }

            self.itemDict[_item.key] = _item;
            _itemFirst.audioSource.volume = 0;
            _itemFirst.audioSource.DOFade(self.volume, duration);
            _itemFirst.isFade = true;
        }

        /// <summary>
        /// 停止标签对应的播放任务，标签对应多个播放任务则全部停止。
        /// </summary>
        /// <param name="key">声音标签,null标签停止所有无标签的播放任务</param>
        public static void Stop(this SoundComponent self,object key)
        {
            AudioSourceComponent _item;
            if (self.itemDict.TryGetValue(key, out _item))
            {
                //需处理整个链表
                AudioSourceComponent _itemNext = null;
                while (_item != null)
                {
                    _itemNext = _item.next;
                    self.RecycleItem(_item);
                    _item = _itemNext;
                }

                self.itemDict.Remove(key);
            }
        }

        /// <summary>
        /// 停止所有播放任务
        /// </summary>
        public static void StopAll(this SoundComponent self)
        {
            foreach (AudioSourceComponent _itemIn in self.items)
            {
                if (_itemIn.audioSource.enabled)
                {
                    if (_itemIn.audioSource.isPlaying)
                    {
                        _itemIn.audioSource.Stop();
                    }

                    if (_itemIn.key != null)
                    {
                        _itemIn.key = null;
                        _itemIn.previous = null;
                        _itemIn.next = null;
                    }

                    //归还
                    _itemIn.audioSource.clip = null;
                    _itemIn.audioSource.enabled = false;
                    self.waitItems.Enqueue(_itemIn);
                }
            }

            self.itemDict.Clear();
        }

        /// <summary>
        /// 判断是否有标签对应的播放任务在播放，标签对应多个播放任务则任意一个在播放返回true
        /// </summary>
        /// <param name="key">声音的标签</param>
        /// <returns>返回是否在播放</returns>
        public static bool IsPlaying(this SoundComponent self,object key)
        {
            AudioSourceComponent _item = null;
            if (self.itemDict.TryGetValue(key, out _item))
            {
                if (_item.audioSource.isPlaying)
                {
                    return true;
                }
                else if (_item.next != null)
                {
                    AudioSourceComponent _itemNext = null;
                    _item = _item.next;
                    while (_item != null)
                    {
                        if (_item.audioSource.isPlaying)
                        {
                            return true;
                        }
                        _itemNext = _item.next;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 是否有播放任务在播放中
        /// </summary>
        /// <returns>组件上有任何播放任务在播放则返回true</returns>
        public static bool IsPlaying(this SoundComponent self)
        {
            AudioSourceComponent _item = null;

            for (int i = 0; i < self.items.Count; i++)
            {
                _item = self.items[i];
                if (_item.audioSource.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
