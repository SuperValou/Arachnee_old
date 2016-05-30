﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class JoliBrowser : MonoBehaviour 
{
    
    private DatabaseDialoger dlg;
    private OnlineRetriever onlret = new OnlineRetriever();

    private List<PrettyFolder> folders = new List<PrettyFolder>();
    private List<Text> sectionTexts = new List<Text>();
    private Dictionary<string, List<PrettyFolder>> arrangement = new Dictionary<string, List<PrettyFolder>>();
    
    private List<string> genres = new List<string>();

    public Canvas c;
    public PrettyFolder prettyFolderPrefab;

    public Vector2 globalOffset;
    public Vector2 localOffset;
    public Vector2 spacing;
    public float spacingSection;

    public Text sectionPrefab;
    public Toggle movieTog;
    public Toggle serieTog;
    public Toggle seenTog;
    private bool mvTogWasOn = false;

    public float scrollspeed = 100;

    void Start()
    {
        this.mvTogWasOn = movieTog.isOn;

        this.dlg = new DatabaseDialoger();
        this.getFolders();
        this.getAllGenres();
        this.SortByTitle();
    }


	
    // sort by genres
    public void SortByGenres()
    {
        Debug.LogError("not implemented yet, p.Genres.Count == 0");

        this.arrangement.Clear();

        foreach (string g in this.genres)
        {
            List<PrettyFolder> l = new List<PrettyFolder>();
            foreach (PrettyFolder p in folders)
            {                
                if (p.Genres.Contains(g))
                {
                    l.Add(p);
                }
            }
            if (l.Count > 0)
            {
                this.arrangement.Add(g, l);
            }            
        }
        
        this.Show();
    }

    // sort by year, from most recent to oldest
    public void SortByYear()
    {
        this.arrangement.Clear();

        this.folders = this.folders.OrderBy(f => f.Year).ToList();

        for (int i = folders.Count - 1; i >= 0; i--)
        {
            string date = ((folders[i].Year / 10) * 10).ToString() + "-" + ((folders[i].Year / 10) * 10 + 9).ToString();

            if(!this.arrangement.ContainsKey(date))
            {
                this.arrangement.Add(date, new List<PrettyFolder>());
            }
            this.arrangement[date].Add(this.folders[i]);
        }
        
        this.Show();
    }

    // sort alphabeticaly by title
    public void SortByTitle()
    {
        this.arrangement.Clear();
        
        List<PrettyFolder> others = new List<PrettyFolder>();

        this.folders = this.folders.OrderBy(f => f.Title).ToList();
        foreach (PrettyFolder p in this.folders)
        {
            string c = p.Title.ToUpper()[0].ToString();

            if (this.arrangement.ContainsKey(c))
            {
                this.arrangement[c].Add(p);
            }
            else if (System.Char.IsLetter(c, 0) || System.Char.IsNumber(c, 0))
            {
                this.arrangement.Add(c, new List<PrettyFolder>());
                this.arrangement[c].Add(p);
            }
            else
            {
                others.Add(p);
            }
        }

        if (others.Count > 0)
        {
            this.arrangement.Add("Others", others);
        }

        this.Show();
    }


    private void getFolders()
    {
        Debug.LogWarning("getFolders should be upgraded! Genres and isSerie are not here!");

        List<PrettyFolder> list = new List<PrettyFolder>();

        // movies
        var set = this.dlg.GetDataSet("SELECT title, year, poster_path, seen, hash, quality, id "
        + "FROM Movies INNER JOIN FoldersM ON Movies.id = FoldersM.idMovie").Tables[0].Rows;

        foreach (System.Data.DataRow row in set)
        {
            PrettyFolder pf = Instantiate(this.prettyFolderPrefab) as PrettyFolder;
            pf.transform.SetParent(c.transform);

            pf.Title = (string)row[0];
            pf.Year = (int)(long)row[1];
            pf.PosterPath = (string)row[2];
            pf.Seen = ((long)row[3] != 0);
            pf.Hash = (string)row[4];
            pf.Quality = (string)row[5];
            pf.Id = (int)(long)row[6];
            
            pf.IsSerie = false;

            pf.info.text = pf.Title + "\n" + pf.Year.ToString() + "\n" + pf.Quality;
            list.Add(pf);
        }

        // series
        set = this.dlg.GetDataSet("SELECT title, startYear, poster_path, seen, hash, id "
        + "FROM Series INNER JOIN FoldersS ON Series.id = FoldersS.idSerie").Tables[0].Rows;

        foreach (System.Data.DataRow row in set)
        {
            PrettyFolder pf = Instantiate(this.prettyFolderPrefab) as PrettyFolder;
            pf.transform.SetParent(c.transform);

            pf.Title = (string)row[0];
            pf.Year = (int)(long)row[1];
            pf.PosterPath = (string)row[2];
            pf.Seen = ((long)row[3] != 0);
            pf.Hash = (string)row[4];
            pf.Id = (int)(long)row[5];
            pf.IsSerie = true;

            pf.info.text = pf.Title + "\n" + pf.Year.ToString() + "\n" + pf.Quality;
            list.Add(pf);
        }

        this.folders = list;

        StartCoroutine(fancy());
    }

    private IEnumerator fancy()
    {
        foreach (PrettyFolder pf in this.folders)
        {
            string imgPath = Application.dataPath + "/Posters/" + pf.PosterPath.Replace("/", "");
            Texture2D tex;

            if (!File.Exists(imgPath))
            {
                yield return StartCoroutine(this.onlret.RetrievePoster(pf.PosterPath));
                if (onlret.Texture != null)
                {
                    var otex = onlret.Texture;
                    File.WriteAllBytes(imgPath, otex.EncodeToJPG());
                }
            }

            if (File.Exists(imgPath))
            {
                tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(imgPath));
            }
            else
            {
                tex = Resources.Load(Constants.Res_DefaultImage) as Texture2D;
            }

            pf.image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

            yield return new WaitForEndOfFrame();
        }
    }

    private void getAllGenres()
    {
        HashSet<string> s = this.dlg.GetAllGenres();

        this.genres = new List<string>(s);
        this.genres.Sort();
    }

    // display the folders on screen
    public void Show()
    {
        // tricky MacTrickFace
        if (!movieTog.isOn && !serieTog.isOn)
        {
            if (mvTogWasOn)
            {
                serieTog.isOn = true;
            }
            else
            {
                movieTog.isOn = true;
            }
            return;
        }
        mvTogWasOn = movieTog.isOn;

        // init values for arrangement
        var rectF = this.prettyFolderPrefab.GetComponent<RectTransform>().rect;
        float prefWidth = rectF.width;
        float prefHeight = rectF.height;
        int maxPerRow = (int)Mathf.Max(1, ((Screen.width - localOffset.x - globalOffset.x - prefWidth / 2) / (prefWidth + spacing.x)));
        Debug.Log(maxPerRow);
        

        var rectS = this.sectionPrefab.GetComponent<RectTransform>().rect;
        float sectPrefW = rectS.width;
        float sectPrefH = rectS.height;

        int sectionIndex = 0;
        float cumulativeHeightOfSections = 0;

        // clear the sections
        foreach (Text t in this.sectionTexts)
        {
            Destroy(t);
        }
        this.sectionTexts.Clear();

        // set the arrangement
        foreach (string key in this.arrangement.Keys)
        {
            // section name
            Text sectionName = Instantiate(this.sectionPrefab) as Text;
            sectionName.transform.SetParent(c.transform);
            sectionName.text = key + "\n____________________";
            sectionName.transform.position = new Vector3(sectPrefW / 2, Screen.height - sectPrefH / 2);
            this.sectionTexts.Add(sectionName);

            // offset
            sectionName.transform.Translate(globalOffset.x, globalOffset.y - sectionIndex * spacingSection - cumulativeHeightOfSections, 0);

            int i = 0;
            int j = 0;
            foreach (PrettyFolder pf in this.arrangement[key])
            {
                // check if need to be shown
                if (!seenTog.isOn && pf.Seen
                    || !movieTog && !pf.IsSerie
                    || !serieTog && pf.IsSerie)
                {
                    pf.gameObject.SetActive(false);
                    continue;
                }
                pf.gameObject.SetActive(true);

                // set position
                pf.transform.position = new Vector3(
                    i * (prefWidth + spacing.x) + prefWidth / 2,
                    Screen.height - j * (prefHeight + spacing.y) - prefHeight / 2,
                    0);

                // offset
                pf.transform.Translate(globalOffset.x, globalOffset.y - sectionIndex * spacingSection - cumulativeHeightOfSections, 0);
                pf.transform.Translate(localOffset.x, localOffset.y, 0);

                // next step
                i++;
                if (i >= maxPerRow)
                {
                    i = 0;
                    j++;
                }
            }

            // not sure of what i'm doin'
            if (i == 0 && j == 0)
            {
                sectionName.gameObject.SetActive(false);
                Debug.Log("nothing in section " + key);
                continue;
            }

            // spacing depending on the size of the section               
            if (i>0)
            {
                j++;
            }
            cumulativeHeightOfSections += rectS.height + j * (rectF.height + spacing.y);
            

            // next section
            sectionIndex++;
        }
    }


    void Update()
    {
        var d = -Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetMouseButtonDown(1))
        {
            this.scrollspeed *= 4;
        }
        if (Input.GetMouseButtonUp(1))
        {
            this.scrollspeed /= 4;
        }

        foreach (PrettyFolder fp in this.folders)
        {
            fp.transform.Translate(scrollspeed * d * Vector3.up);
        }
        foreach (Text t in this.sectionTexts)
        {
            t.transform.Translate(scrollspeed * d * Vector3.up);
        }
    }
}
