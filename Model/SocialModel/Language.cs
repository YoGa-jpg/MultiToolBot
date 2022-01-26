using System;
using System.Linq;

namespace MultiToolBot.Model.SocialModel
{
    public class Language
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Short
        {
            get => String.Concat(Name.Take(3));
        }
    }
}