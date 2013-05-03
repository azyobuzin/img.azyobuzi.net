# -*- coding: utf-8 -*-

import json
import urllib2

from resolvers import *

class TwitCasting(StoringResolver):
    @property
    def service_name(self):
        return "TwitCasting"
    
    @property
    def regex_str(self):
       return r"^https?://(?:www\.)?twitcasting\.tv/(?:(\w+)/?|\w+/movie/(\d+))(?:\?.*)?$"
    
    def get_parameters(self, match):
        return match.group(2)

    def get_thumbnail(self, match):
        username = match.group(1)
        return "http://twitcasting.tv/%s/thumbstream/liveshot" % username if username else self.work(match)["thumbnail"]

    def get_thumbnailsmall(self, match):
        username = match.group(1)
        return "http://twitcasting.tv/%s/thumbstream/liveshot-1" % username if username else self.work(match)["thumbnailsmall"]
    
    def _work(self, param, cursor):
        table = "twitcasting"
        columns = ["id", "thumbnail", "thumbnailsmall"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]
        
        response = urllib2.urlopen("http://api.twitcasting.tv/api/moviestatus?type=json&movieid=" + param)
        content = response.read()
            
        if content == "[]":
            raise PictureNotFoundError()
            
        j = json.loads(content)
            
        thumbnail = j["thumbnail"]
        thumbnailsmall = j["thumbnailsmall"]
            
        self.insert_all(cursor, table, (param, thumbnail, thumbnailsmall))
        return dict(zip(columns[1:], (thumbnail, thumbnailsmall)))
    
    def get_full(self, match):
        return self.get_thumbnail(match)

    def get_full_https(self, match):
        return self.get_full(match) \
            .replace("http://movie.", "https://ssl.", 1) \
            .replace("http://", "https://ssl.", 1)
    
    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)
    
    def get_thumb(self, match):
        return self.get_thumbnailsmall(match)

    def get_thumb_https(self, match):
        return self.get_thumb(match) \
            .replace("http://movie.", "https://ssl.", 1) \
            .replace("http://", "https://ssl.", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
