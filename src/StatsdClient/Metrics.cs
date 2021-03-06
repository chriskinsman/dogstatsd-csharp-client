﻿using System;

namespace StatsdClient
{
    public static class Metrics
    {
        private static Statsd _statsD;
        private static string _prefix;
        private static string[] _tags;

        public static void Configure(MetricsConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(config.StatsdServerName))
                throw new ArgumentNullException("config.StatsdServername");

            _prefix = config.Prefix;
            _tags = config.Tags;
            _statsD = string.IsNullOrEmpty(config.StatsdServerName)
                      ? null
                      : new Statsd(new StatsdUDP(config.StatsdServerName, config.StatsdPort, config.StatsdMaxUDPPacketSize));
        }

        public static void Counter<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting,T>(BuildNamespacedStatName(statName), value, sampleRate, MergeTags(tags));
        }

        public static void Increment(string statName, double sampleRate = 1.0, string[] tags = null)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting, int>(BuildNamespacedStatName(statName), 1, sampleRate, MergeTags(tags));
        }

        public static void Decrement(string statName, double sampleRate = 1.0, params string[] tags)  
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Counting, int>(BuildNamespacedStatName(statName), -1, sampleRate, MergeTags(tags));
        }

        public static void Gauge<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Gauge, T>(BuildNamespacedStatName(statName), value, sampleRate, MergeTags(tags));
        }

        public static void Histogram<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            if (_statsD == null)
            {
                return;
            }
            _statsD.Send<Statsd.Histogram, T>(BuildNamespacedStatName(statName), value, sampleRate, MergeTags(tags));
        }

        public static void Set<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            if (_statsD == null) 
            {
                return;
            }
            _statsD.Send<Statsd.Set, T>(BuildNamespacedStatName(statName), value, sampleRate, MergeTags(tags));
        }

        public static void Timer<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            if (_statsD == null)
            {
                return;
            }

            _statsD.Send<Statsd.Timing, T>(BuildNamespacedStatName(statName), value, sampleRate, MergeTags(tags));
        }


        public static IDisposable StartTimer(string name, double sampleRate = 1.0, string[] tags = null)
        {
            return new MetricsTimer(name, sampleRate, MergeTags(tags));
        }

        public static void Time(Action action, string statName, double sampleRate = 1.0, string[] tags = null) 
        {
            if (_statsD == null)
            {
                action();
            }
            else
            {
                _statsD.Send(action, BuildNamespacedStatName(statName), sampleRate, MergeTags(tags));
            }
        }

        public static T Time<T>(Func<T> func, string statName, double sampleRate = 1.0, string[] tags = null)
        {
            if (_statsD == null)
            {
                return func();
            }

            using (StartTimer(statName, sampleRate, MergeTags(tags)))
            {
                return func();
            }
        }

        private static string BuildNamespacedStatName(string statName)
        {
            if (string.IsNullOrEmpty(_prefix))
            {
                return statName;
            }

            return _prefix + "." + statName;
        }

        private static string[] MergeTags(string[] tags)
        {
            if (tags == null && _tags == null)
            {
                return null;
            }
            else if (tags == null && _tags != null)
            {
                return _tags;
            }
            else if (tags != null && _tags == null)
            {
                return tags;
            }
            else
            {
                string[] mergedTags = new string[tags.Length + _tags.Length];
                _tags.CopyTo(mergedTags, 0);
                tags.CopyTo(mergedTags, _tags.Length);

                return mergedTags;
            }
                
        }
    }
}
