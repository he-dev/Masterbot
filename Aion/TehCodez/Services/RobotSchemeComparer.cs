﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aion.Data;

namespace Aion.Services
{
    public static class RobotSchemeComparer
    {
        private static readonly IEqualityComparer<ProcessGroup> RobotSchemeFileNameComparer = new RobotSchemeFileNameComparer();

        public static IEnumerable<(string RobotScheme, ScheduleAction FileAction)> Compare(this ICollection<ProcessGroup> current, ICollection<ProcessGroup> other)
        {

            foreach (var item in current)
            {
                var match = other.SingleOrDefault(y => RobotSchemeFileNameComparer.Equals(item, y));
                if (match == null)
                {
                    yield return (item, ScheduleAction.Remove);
                }
                else
                {
                    yield return
                        item.Equals(match)
                            ? (item, ScheduleAction.None)
                            : (match, ScheduleAction.Replace);
                }
            }

            foreach (var item in other)
            {
                if (!current.Any(x => RobotSchemeFileNameComparer.Equals(item, x)))
                {
                    yield return (item, ScheduleAction.Add);
                }
            }
        }
    }

    public class RobotSchemeFileNameComparer : IEqualityComparer<ProcessGroup>
    {
        public bool Equals(ProcessGroup x, ProcessGroup y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                string.Equals(x.FileName, y.FileName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(ProcessGroup obj)
        {
            return obj.FileName.ToLowerInvariant().GetHashCode();
        }
    }

    public enum ScheduleAction
    {
        None,
        Replace,
        Add,
        Remove,
    }
}
