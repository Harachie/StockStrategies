﻿Public Class Downloader

    Public Function DownloadArivaDividends(arrivaStockName As String) As String
        Return DownloadCachedMonthly(String.Format("http://www.ariva.de/{0}-aktie/historische_ereignisse?clean_split=1", arrivaStockName))
    End Function

    Public Function DownloadStooqData(meta As StockMetaData) As String
        Return DownloadStooqData(meta.StooqTicker)
    End Function

    Public Function DownloadStooqData(stooqTicker As String) As String 'https://stooq.com/q/d/?s=alv.de&c=0&l=6&o=0111111&o_d=1&o_p=1&o_n=1&o_o=1&o_m=1&o_x=1
        Return DownloadCachedMonthly(String.Format("https://stooq.com/q/d/l/?s={0}&i=d&o=0111111", stooqTicker))
    End Function

    Public Function DownloadCachedDaily(url As String) As String
        Return DownloadCached(url, "yyyyMMdd")
    End Function

    Public Function DownloadCachedMonthly(url As String) As String
        Return DownloadCached(url, "yyyyMM")
    End Function

    Public Function DownloadCached(url As String, dateFormat As String) As String
        Dim fileName As String
        Dim filePath As String
        Dim client As Net.WebClient
        Dim content As String

        fileName = Date.UtcNow.ToString(dateFormat) & "_" & url.Sha1
        filePath = IO.Path.Combine(GetCacheDirectory(), fileName)

        If IO.File.Exists(filePath) Then
            Return IO.File.ReadAllText(filePath)
        End If

        client = New Net.WebClient()
        content = client.DownloadString(url)
        IO.File.WriteAllText(filePath, content)

        Return content
    End Function

End Class
