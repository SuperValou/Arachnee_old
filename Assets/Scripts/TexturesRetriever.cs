﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TexturesRetriever : OnlineRetriever
{
    protected override IEnumerator RetrieveDataImpl(HashSet<string> identifiers)
    {
        var posterPaths = identifiers.Where(p => !string.IsNullOrEmpty(p)).ToList();

        // load texture from disk if possible
        foreach (var path in posterPaths.Where(TextureIsOnDisk))
        {
            try
            {
                var texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(GetFilePathFromPoster(path)));
                _retrievedData.Add(path, texture);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Couldn't load poster from disk, details: " + e.Message);
            }
        }

        // load texture from internet
        foreach (var path in posterPaths.Where(p => !_retrievedData.Keys.Contains(p)))
        {
            using (var www = new WWW(Constants.posterUrl + path))
            {
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError("Error when retrieving poster " + path + ". Details: " + www.error);
                    yield break;
                }

                // add texture to disk if possible
                try
                {
                    File.WriteAllBytes(GetFilePathFromPoster(path), www.texture.EncodeToJPG());
                    _retrievedData.Add(path, www.texture);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Couldn't save poster to disk, details: " + e.Message);
                }
            }
        }

        // load default texture
        foreach (var path in posterPaths.Where(p => !_retrievedData.Keys.Contains(p)))
        {
            _retrievedData.Add(path, GetDefaultTexture());
        }
    }
    
    /// <summary>
    /// Return the default texture
    /// </summary>
    public static Texture2D GetDefaultTexture()
    {
        return Resources.Load(Constants.Res_DefaultImage) as Texture2D;
    }

    private bool TextureIsOnDisk(string posterPath)
    {
        if (string.IsNullOrEmpty(posterPath))
        {
            return false;
        }
        return File.Exists(GetFilePathFromPoster(posterPath));
    }

    private string GetFilePathFromPoster(string posterPath)
    {
        string filename;
        if (string.IsNullOrEmpty(posterPath))
        {
            filename = Path.GetRandomFileName();
        }
        else
        {
            filename = Path.GetInvalidFileNameChars().Aggregate(posterPath,
                (current, invalidFileNameChar) => current.Replace(invalidFileNameChar, '_'));
        }
        return Path.Combine(Path.Combine(Application.dataPath, "Posters"), filename).Replace('\\', '/');
    }
}

