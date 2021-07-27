namespace BlobStaticFileServerFunctions.Options
{
	public class BasicAuthOptions
	{
		public bool Bypass { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		private string header;
		public string Header
		{
			get
			{
				return header ?? SetHeader();
			}
		}

		private string SetHeader()
		{
			header = $"{Username}:{Password}";
			return header;
		}
	}
}
