namespace Client
{
    public enum FileDownloadState
    {
        None, 
        //загрузить
        Downloading, 
        //анимация загрузка
        Downloaded, 
        //открыть
        Uploadind 
        //анимация загрузка
    }
}