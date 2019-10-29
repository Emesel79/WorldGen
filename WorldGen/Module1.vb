Imports System.Drawing
Imports Substrate
Imports Substrate.Nbt
Imports Substrate.Core
Imports System.IO

Module Module1
    Public PostCode As String
    Public Easting As String
    Public Northing As String
    Public Centre As String
    Public size As Integer = 256
    Public W As Integer
    Public H As Integer
    Public AnchorE As Integer
    Public AnchorN As Integer
    Public LevelName As String

    Public xmin As Integer = -8
    Public xmax As Integer = 8
    Public zmin As Integer = -8
    Public zmaz As Integer = 8
    Public Blockx As Integer = 0
    Public Blockz As Integer = 0


    Sub main()
        LevelName = "MarkTest"
        PostCode = "DL57BB"

        Dim dest As String = "C:\Data\MC\" & LevelName 'args(1)

        If Not Directory.Exists(dest) Then Directory.CreateDirectory(dest)
        Dim world As NbtWorld
        world = AnvilWorld.Create(dest)

        postcode_to_en()

        W = size : H = size
        AnchorE = CInt(Easting)
        AnchorN = CInt(Northing)

        Console.WriteLine("Setting World Options")
        world.Level.LevelName = LevelName
        world.Level.Player = New Player()
        world.Level.Player.Abilities.Flying = 0
        world.Level.Spawn = New SpawnPoint(Int(W / 2), 250, Int(H / 2))
        world.Save()

        Dim cm As IChunkManager = world.GetChunkManager()
        For xi As Integer = xmin To xmax - 1

            For zi As Integer = zmin To zmaz - 1
                Dim chunk As ChunkRef = cm.CreateChunk(xi, zi)
                chunk.IsTerrainPopulated = True
                chunk.Blocks.AutoLight = False
                FlatChunk(chunk, 64)
                chunk.Blocks.RebuildHeightMap()
                chunk.Blocks.RebuildBlockLight()
                chunk.Blocks.RebuildSkyLight()
                Console.WriteLine("Built Chunk {0},{1}", chunk.X, chunk.Z)
                cm.Save()
            Next
        Next

        world.Save()

        Console.ReadLine()
    End Sub

    Sub postcode_to_en()
        Console.WriteLine("Getting postcode from http://data.ordnancesurvey.co.uk/doc/postcodeunit/" & PostCode & ".json")
        Dim json As String = New System.Net.WebClient().DownloadString("http://data.ordnancesurvey.co.uk/doc/postcodeunit/" & PostCode & ".json")

        Dim ePos As Integer = json.IndexOf("http://data.ordnancesurvey.co.uk/ontology/spatialrelations/easting")
        Dim nPos As Integer = json.IndexOf("http://data.ordnancesurvey.co.uk/ontology/spatialrelations/northing")

        Easting = Mid(json, ePos + 120, 6)
        Northing = Mid(json, nPos + 121, 6)
        Centre = Easting & ", " & Northing
        Console.WriteLine("Easting: " & Easting & " | Northing: " & Northing)

    End Sub

    Private Sub FlatChunk(ByVal chunk As ChunkRef, ByVal height As Integer)



        For y As Integer = 0 To 2 - 1

            For x As Integer = 0 To 16 - 1

                For z As Integer = 0 To 16 - 1
                    chunk.Blocks.SetID(x, y, z, CInt(BlockType.BEDROCK))
                Next
            Next
        Next

        For y As Integer = 2 To height - 5 - 1

            For x As Integer = 0 To 16 - 1

                For z As Integer = 0 To 16 - 1
                    chunk.Blocks.SetID(x, y, z, CInt(BlockType.STONE))
                Next
            Next
        Next

        For y As Integer = height - 5 To height - 1 - 1

            For x As Integer = 0 To 16 - 1

                For z As Integer = 0 To 16 - 1

                    chunk.Blocks.SetID(x, y, z, CInt(BlockType.DIRT))
                Next
            Next
        Next

        For y As Integer = height - 1 To height - 1

            For x As Integer = 0 To 16 - 1
                If xmin < 0 Then Blockx = xmin * -1 Else Blockx = xmin
                Blockx = ((chunk.X + Blockx) * 16) + x
                For z As Integer = 0 To 16 - 1
                    If zmin < 0 Then Blockz = zmin * -1 Else Blockz = zmin
                    Blockz = ((chunk.Z + Blockz) * 16) + z
                    chunk.Blocks.SetID(x, y, z, CInt(getblock(Blockx, Blockz)))

                Next

            Next
        Next
    End Sub

    Function getblock(xx, zz)
        'https://tiles.wmflabs.org/osm-no-labels/18/129916/83397.png
        Dim img As Bitmap = New Bitmap("C:\DATA\geocraft-master\var\tiles\18_129916_83397.png")
        Dim pixel As Color = img.GetPixel(xx, zz)
        Dim colour As String
        colour = pixel.R.ToString & pixel.G.ToString & pixel.B.ToString


        Select Case colour
            Case "224223223", "216215215", "206205205" 'Stone
                getblock = BlockType.STONE
            Case "247222217", "247222217", "249188180", "249142130", "248203196" 'Path
                getblock = BlockType.GRAVEL
            Case "220235238", "181217227", "170211223", "197225233", "187220229", "213232237", "174212224" 'Water
                getblock = BlockType.STATIONARY_WATER
            Case "190234186", "177213164", "181220171", "173209158", "157192143", "147182133" 'Tall Grass
                getblock = BlockType.TALL_GRASS
            Case Else
                getblock = BlockType.GRASS
        End Select
        Console.WriteLine(colour & " " & xx & " " & zz & " " & getblock.ToString)
        Return getblock
    End Function

    Private Function CalcTileXY(ByVal lat As Single, ByVal lon As Single, ByVal zoom As Long) As Point

        ' http://dev.openstreetmap.org/~ojw/Tiles/tile.php/14/8452/5496.png

        Dim xf As Single = (lon + 180) / 360 * 2 ^ zoom
        Dim yf As Single = (1 - Math.Log(Math.Tan(lat * Math.PI / 180) + 1 / Math.Cos(lat * Math.PI / 180)) / Math.PI) / 2 * 2 ^ zoom
        CalcTileXY.X = CLng(Math.Floor(xf))
        CalcTileXY.Y = CLng(Math.Floor(yf))

    End Function
End Module

