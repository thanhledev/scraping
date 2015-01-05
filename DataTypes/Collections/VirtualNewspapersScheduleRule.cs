using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Enums;
using DataTypes.Collections;

namespace DataTypes.Collections
{
    public class VirtualNewspapersScheduleRule
    {
        #region variables

        private int _numberOfPostsPerDay;

        public int NumberOfPostsPerDay
        {
            get { return _numberOfPostsPerDay; }
            set { _numberOfPostsPerDay = value; }
        }

        private TimeBetweenPost _timeBetweenPost;

        public TimeBetweenPost TimeBetweenPost
        {
            get { return _timeBetweenPost; }
            set { _timeBetweenPost = value; }
        }

        private int _minInterval;

        public int MinInterval
        {
            get { return _minInterval; }
            set { _minInterval = value; }
        }

        private int _maxInterval;

        public int MaxInterval
        {
            get { return _maxInterval; }
            set { _maxInterval = value; }
        }

        private TimeUnit _timeUnit;

        public TimeUnit TimeUnit
        {
            get { return _timeUnit; }
            set { _timeUnit = value; }
        }

        private int _comebackInterval;

        public int ComebackInterval
        {
            get { return _comebackInterval; }
            set { _comebackInterval = value; }
        }
       
        #endregion

        #region constructors

        public VirtualNewspapersScheduleRule()
            : this(0, TimeBetweenPost.Automatically, 0, 0, TimeUnit.Minute, 30)
        {

        }

        public VirtualNewspapersScheduleRule(int numberOfPostsPerDay, TimeBetweenPost timeBetweenPost, int minInterval, int maxInterval, TimeUnit timeUnit, int comebackInterval)
        {
            _numberOfPostsPerDay = numberOfPostsPerDay;
            _timeBetweenPost = timeBetweenPost;
            _minInterval = minInterval;
            _maxInterval = maxInterval;
            _timeUnit = timeUnit;
            _comebackInterval = comebackInterval;
        }

        #endregion

        #region UtilityMethods



        #endregion
    }
}
