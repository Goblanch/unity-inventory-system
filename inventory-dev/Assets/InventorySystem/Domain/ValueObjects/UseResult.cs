namespace GB.Inventory.Domain
{
    /// <summary>
    /// Resultado de usar un item: éxito / fallo y si debe consumirse una unidad
    /// </summary>
    public struct UseResult
    {
        public bool Success;
        public bool ConsumeOne;
        public string Message;

        public static UseResult OkConsume(string msg = null) => new UseResult { Success = true, ConsumeOne = true, Message = msg };
        public static UseResult OkKeep(string msg = null) => new UseResult { Success = true, ConsumeOne = false, Message = msg };
        public static UseResult Fail(string msg) => new UseResult { Success = false, ConsumeOne = false, Message = msg };
    }
}