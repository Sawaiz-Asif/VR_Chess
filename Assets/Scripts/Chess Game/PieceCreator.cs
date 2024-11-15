using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceCreator : MonoBehaviour
{
    [SerializeField] private GameObject[] piecePrefabs;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material whiteMaterial;
        
    private Dictionary<string, GameObject> nameToPieceDict = new Dictionary<string, GameObject>();

    private void Awake(){
        foreach(var piece in piecePrefabs){
            nameToPieceDict.Add(piece.GetComponent<Piece>().GetType().ToString(), piece);
        }
    }

    public GameObject CreatePiece(Type type){
        GameObject prefab = nameToPieceDict[type.ToString()];
        if (prefab){
            GameObject newPiece = Instantiate(prefab);
            // Check if the piece has a collider, if not, add one
            if (newPiece.GetComponent<Collider>() == null)
            {
                newPiece.AddComponent<BoxCollider>();
            }
            return newPiece;
        }
        return null;
    }

    public Material GetTeamMaterial(TeamColor team){
        return team == TeamColor.White ? whiteMaterial : blackMaterial;
    }
}
