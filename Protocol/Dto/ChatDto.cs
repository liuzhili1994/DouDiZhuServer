using System;
namespace Protocol.Dto
{
    [Serializable]
    public class ChatDto
    {
        public int userId;

        public int chatType;

        public ChatDto()
        {
        }

        public ChatDto(int userId,int chatType)
        {
            this.userId = userId;
            this.chatType = chatType;
        }
    }
}
