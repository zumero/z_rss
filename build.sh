#!/bin/sh

gmcs -reference:System.ServiceModel.Web.dll -out:z_rss_create.exe ../sqlite-net/src/SQLite.cs z_rss_create.cs
gmcs -reference:System.ServiceModel.Web.dll -out:z_rss_update.exe ../sqlite-net/src/SQLite.cs z_rss_update.cs
gmcs -reference:System.ServiceModel.Web.dll -out:z_rss_sync.exe ../sqlite-net/src/SQLite.cs z_rss_sync.cs

