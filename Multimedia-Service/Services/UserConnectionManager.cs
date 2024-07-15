namespace MultimediaService.Services
{
    public class UserConnectionManager
    {
        private readonly Dictionary<string, string> _connections = new Dictionary<string, string>();

        public void AddConnection(string username, string connectionId)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(username))
                {
                    _connections[username] = connectionId;
                }
                else
                {
                    _connections.Add(username, connectionId);
                }
            }
        }

        public string GetConnectionId(string username)
        {
            lock (_connections)
            {
                return _connections.ContainsKey(username) ? _connections[username] : null;
            }
        }

        public void RemoveConnection(string connectionId)
        {
            lock (_connections)
            {
                var item = _connections.FirstOrDefault(kvp => kvp.Value == connectionId);
                if (!item.Equals(default(KeyValuePair<string, string>)))
                {
                    _connections.Remove(item.Key);
                }
            }
        }
    }
}
