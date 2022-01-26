namespace MultiToolBot.Model.SocialModel
{
    class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int Tag { get; set; }
        public string Languages { get; set; }
        public string Games { get; set; }
        public string Form { get; set; }
        public long? GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
