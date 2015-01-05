using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTypes.Enums;
using DataTypes.Collections;

namespace DataTypes.Collections
{
    public class ScheduleRule
    {
        #region variables

        private ScheduleMode _scheduleMode;

        public ScheduleMode ScheduleMode
        {
            get { return _scheduleMode; }
            set { _scheduleMode = value; }
        }

        private int _numberOfPosts;

        public int NumberOfPosts
        {
            get { return _numberOfPosts; }
            set { _numberOfPosts = value; }
        }

        private TimeRange _timeRange;

        public TimeRange TimeRange
        {
            get { return _timeRange; }
            set { _timeRange = value; }
        }

        private int _numberOfDays;

        public int NumberOfDays
        {
            get { return _numberOfDays; }
            set { _numberOfDays = value; }
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

        private int _searchDepth;

        public int SearchDepth
        {
            get { return _searchDepth; }
            set { _searchDepth = value; }
        }

        private Limitation _limitation;

        public Limitation Limitation
        {
            get { return _limitation; }
            set { _limitation = value; }
        }

        private DateTime _startDate;

        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        private DateTime _endDate;

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        #endregion

        #region constructors

        public ScheduleRule()
            :this(ScheduleMode.LiveFeed, 0, TimeRange.Daily, 0, TimeBetweenPost.Automatically, 0, 0, TimeUnit.Minute, 0, 0, Limitation.Unlimited)
        {

        }

        public ScheduleRule(ScheduleMode scheduleMode, int numberOfPosts, TimeRange timeRange, int numberOfDays,
                        TimeBetweenPost timeBetweenPost, int minInterval, int maxInterval, TimeUnit timeUnit, int comebackInterval, int searchDepth, Limitation limit)
        {
            _scheduleMode = scheduleMode;
            _numberOfPosts = numberOfPosts;
            _timeRange = timeRange;
            _numberOfDays = numberOfDays;
            _timeBetweenPost = timeBetweenPost;
            _minInterval = minInterval;
            _maxInterval = maxInterval;
            _timeUnit = timeUnit;
            _comebackInterval = comebackInterval;
            _searchDepth = searchDepth;
            _limitation = limit;
        }

        #endregion

        #region UtilityMethods
      
        #endregion
    }
}
