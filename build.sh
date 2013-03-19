#!/bin/sh

gmcs -reference:System.ServiceModel.Web.dll -out:z_rss_update.exe ../sqlite-net/src/SQLite.cs z_rss_update.cs

