using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CyberLife.Board {
  public class BoardController : MonoBehaviour {
    public RectTransform tilesRoot, pawnsRoot;
    [Range(4,32)] public int side=8; public float tileSize=80f, gap=6f;
    public Color tileColor=new Color(.15f,.15f,.15f,.9f), tileAltColor=new Color(.22f,.22f,.22f,.9f);
    public List<RectTransform> tiles=new List<RectTransform>();
    public int Perimeter => Mathf.Max(0, side*4-4);
    [ContextMenu("Generate Board")] public void Generate(){
      if(!tilesRoot){Debug.LogError("[Board] tilesRoot is null");return;}
      for(int i=tilesRoot.childCount-1;i>=0;i--) { var g=tilesRoot.GetChild(i).gameObject;
        if(Application.isEditor) DestroyImmediate(g); else Destroy(g); }
      tiles.Clear(); if(side<4) side=4;
      int idx=0; for(int x=0;x<side;x++) CreateTile(idx++, Pos(x,0));
      for(int y=1;y<side-1;y++) CreateTile(idx++, Pos(side-1,y));
      for(int x=side-1;x>=0;x--) CreateTile(idx++, Pos(x,side-1));
      for(int y=side-2;y>=1;y--) CreateTile(idx++, Pos(0,y));
    }
    Vector2 Pos(int gx,int gy){ float step=tileSize+gap, half=(side-1)*step*.5f; return new Vector2(gx*step-half, gy*step-half); }
    void CreateTile(int index, Vector2 pos){ var go=new GameObject($"Tile_{index}", typeof(RectTransform), typeof(Image));
      var rt=go.GetComponent<RectTransform>(); rt.SetParent(tilesRoot,false); rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
      rt.pivot=new Vector2(.5f,.5f); rt.sizeDelta=new Vector2(tileSize,tileSize); rt.anchoredPosition=pos;
      go.GetComponent<Image>().color=(index%2==0)?tileColor:tileAltColor; tiles.Add(rt); }
    public Vector2 GetTilePosition(int i){ if(tiles==null||tiles.Count==0) return Vector2.zero; i=Mod(i, tiles.Count); return tiles[i].anchoredPosition; }
    public static int Mod(int a,int n)=>(a%n+n)%n;
  } }