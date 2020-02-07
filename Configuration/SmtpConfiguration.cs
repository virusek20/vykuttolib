namespace vykuttolib.Configuration
{
	class SmtpConfiguration
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string SenderHost { get; set; }
		public string SenderName { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}
