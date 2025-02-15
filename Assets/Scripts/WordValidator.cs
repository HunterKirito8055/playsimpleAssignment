using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WordValidator : MonoBehaviour
{
    public static WordValidator Instance { get; private set; }

    private HashSet<string> wordList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadWordList();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadWordList()
    {

        wordList = new HashSet<string>();

        TextAsset textAsset = Resources.Load<TextAsset>("wordList");
        if (textAsset != null)
        {
            string[] lines = textAsset.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                wordList.Add(line.ToUpper());
            }
        }
    }

    public bool IsValidWord(string word)
    {
        return wordList.Contains(word.ToUpper());
    }
}
