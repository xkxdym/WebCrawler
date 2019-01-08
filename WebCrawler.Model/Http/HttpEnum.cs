namespace WebCrawler.Model.Http
{
    /// <summary>
    /// Post 的数据类型
    /// </summary>
    public enum HttpPostDataType
    {
        /// <summary>
        /// 字符串
        /// </summary>
        String,

        /// <summary>
        /// Byte
        /// </summary>
        Byte,

        /// <summary>
        /// 文件地址
        /// </summary>
        FilePath
    }

    /// <summary>
    /// 请求返回类型
    /// </summary>
    public enum HttpResultType
    {
        /// <summary>
        /// 字符串 只有Html有数据
        /// </summary>
        String,

        /// <summary>
        /// 返回字符串和字节流
        /// </summary>
        StringAndByte
    }
}

