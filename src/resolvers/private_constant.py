# -*- coding: utf-8 -*-

import os
import oauth2

db_host = os.getenv("IMGAZYOBUZI_DB_HOST", "localhost")
db_port = int(os.getenv("IMGAZYOBUZI_DB_PORT", "3306"))
db_name = os.getenv("IMGAZYOBUZI_DB_NAME")
db_user = os.getenv("IMGAZYOBUZI_DB_USER")
db_password = os.getenv("IMGAZYOBUZI_DB_PASSWORD")

consumer_key_500px = "jDMkZjOXcidZZex6lhloa95YRnZDVUQhrxX0IHKv"
flickr_api_key = "341194ea58132794a8d6730b19f92757"
tinami_api_key = "503cb18fee10e"
tumblr_api_key = "Rf22FFPnVuWLa9HaRQCPvIMfvTAZYcVRJXLdsg59lkYsnpsTLU"
ustream_api_key = "E684BEE3166534D6B6A835F62BD535A3"

# @imgazyobuzi・ReadOnly 権限
twitter_client = oauth2.Client(
    oauth2.Consumer(key="uiYQy5R2RJFZRZ4zvSk7A", secret="qzDldacVrcyXbp8pBerf1LBfnQXmkPKmyLVGGLus8"),
    oauth2.Token(key="862962650-rIcjsj0j9ZJ8khPVA8jZTtEJuq7YYDBDpx6fOAgb", secret="kbMQjdVldI6tFOST3SVjmyAtG1D0oCkCpL6vBv1FtA")
)

__all__ = [
    "db_host", "db_port", "db_name", "db_user", "db_password",
    "consumer_key_500px", "flickr_api_key", "tinami_api_key",
    "tumblr_api_key", "ustream_api_key",
    "twitter_client"
]
