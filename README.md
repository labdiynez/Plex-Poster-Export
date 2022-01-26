
![GitHub](https://img.shields.io/github/license/pekempy/NFOBuilder?label=license&style=for-the-badge) ![Maintenance Itended](https://img.shields.io/maintenance/no/2022?style=for-the-badge) ![Language](https://img.shields.io/badge/Language-C%23-green?label=language&style=for-the-badge)

# Plex-Poster-Export
Export Plex Posters for Movie or TV libraries to the directory the media is stored in. 
Requires a csv export from [WebTools-NG](https://github.com/WebTools-NG/WebTools-NG)

---
### You will need:
#### For MOVIES:
* CSV Export (semi-colon delimited) on Movies from [WebTools-NG](https://github.com/WebTools-NG/WebTools-NG) with the following custom fields:
    - Title
    - Poster url
    - Part File Combined
 * Latest download from [releases](https://github.com/pekempy/Plex-Poster-Export/releases)
---
#### For TV SHOWS:
* CSV Export (semi-colon delimited) on TV Episodes from [WebTools-NG](https://github.com/WebTools-NG/WebTools-NG) with  the following custom fields:
    - Series Title
    - Title
    - Season
    - Episode
    - Poster url
    - Part File Combined
 * Latest download from [releases](https://github.com/pekempy/Plex-Poster-Export/releases)
 ---
### How to use:
With the .csv from WebTools-NG and the .exe from here, you can either:
* Double click the .exe and provide the Plex URL (`http://192.168.1.94:32400` as an example) and your Plex Token
* Create a batch file to run the .exe and provide the URL and token as args:     
    ```@echo off```    
    ```C:\Users\Pekempy\Documents\Plex-Poster-Export\PlexPosterExport.exe --u http://192.168.1.94:32400 --t abCDefGHijKlmNOPqRSTuV```    
    ```pause```    

Once you've done this, you will be prompted to drag the CSV into the window (I recommend renaming it to something like `Movies.csv` or `TV.csv` as opposed to the default name from WebTools-NG

It will then begin exporting the posters to your medias file location.

---
### Outputs
For Movies, 3 posters are generated 
* {movie}.png
* {movie}-poster.png
* poster.png

For TV shows, one poster is generated
* {episode}.png

A Log File is also created in the location you run from (bat file or .exe) with the console output, as if there are duplicates you will have these noted here and can fix them. Sometimes also there are errors downloading the poster, which will be listed here.

---
### Use cases
I personally use this to back up my Plex Metadata to file, and keep Plex/Jellyfin posters in sync after customising posters from [ThePosterDB](https://theposterdb.com)    
For TV episodes, I use it to back up custom title cards from [r/PlexTitleCards](https://reddit.com/r/PlexTitleCards)
