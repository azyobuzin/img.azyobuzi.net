# -*- coding: utf-8 -*-

import json
import urllib2

from resolvers import *

class Vimeo(StoringResolver):
    @property
    def service_name(self):
        return "Vimeo"
    
    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?vimeo\.com/(\d+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)
    
    def _work(self, param, cursor):
        table = "vimeo"
        columns = ["id", "small", "large"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))
        
        try:
            response = urllib2.urlopen("http://vimeo.com/api/v2/video/%s.json" % param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e
            
        j = json.load(response)[0]
            
        small = j["thumbnail_small"]
        large = j["thumbnail_large"]
            
        self.insert_all(cursor, table, (param, small, large))
        return dict(zip(columns[1:], (small, large)))
    
    def get_full(self, match):
        return self.work(match)["large"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)
    
    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.work(match)["small"]

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
