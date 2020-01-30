﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EventApp
{
    public static class Utils
    {
         public static string GetCelebrationImage(bool is_celebrating)
        {
            if (is_celebrating)
            {
                return "celebrate_active.png";
            }
            else
            {
                return "celebrate.png";
            }
        }
        public static string GetUpVoteImage(string vote)
        {
            // TODO: consider turning this into a global static app variable
            string upvote_active = "up_active.png";
            string upvote_neutral = "up.png";
            if (vote == "up")
            {
                return upvote_active;
            }
            else
            {
                return upvote_neutral;
            }
        }
        public static string GetDownVoteImage(string vote)
        {
            // TODO: consider turning this into a global static app variable
            string downvote_active = "down_active.png";
            string downvote_neutral = "down.png";
            if (vote == "down")
            {
                return downvote_active;
            }
            else
            {
                return downvote_neutral;
            }
        }
    }
}
