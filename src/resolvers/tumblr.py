# -*- coding: utf-8 -*-

import json
import re
import urllib2

from resolvers import *
from resolvers.private_constant import *

class Tumblr(StoringResolver):
    @property
    def service_name(self):
        return "Tumblr"
    
    @property
    def regex_str(self):
        return r"^http://(?:([\w\-]+\.tumblr\.com)/(?:post|image)/(\d+)(?:/(?:[\w\-]+/?)?)?|tmblr\.co/([\w\-]+))(?:\?.*)?(?:#.*)?$"
    
    expanded_regex = re.compile("^http://([\\w\\.\\-]+)/post/(\\d+)(?:/(?:[\\w\\-]+/?)?)?(?:#.*)?$", re.IGNORECASE)
    
    def get_parameters(self, match):
        return {"hostname": match.group(1), "id": match.group(2), "shorten": match.group(3)}
    
    def _work(self, param, cursor):
        if param["shorten"]:
            table = "tumblr_shorten"
            columns = ["shorten", "hostname", "id"]
            result = self.select_one(cursor, table, columns[1:], {columns[0]: param["shorten"]})
            
            if not result:
                response = urllib2.build_opener(DontRedirectHandler).open(
                    Request2("http://www.tumblr.com/" + param["shorten"], method="HEAD")
                )

                try:
                    match = self.expanded_regex.match(response.getheader("location"))
                except:
                    raise PictureNotFoundError()

                hostname = match.group(1)
                id = match.group(2)
                
                self.insert_all(cursor, table, (param["shorten"], hostname, id))
            else:
                hostname = result[0]
                id = result[1]
        else:
            hostname = param["hostname"]
            id = param["id"]

        table = "tumblr"
        columns = ["id", "original", "large", "thumb", "video"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: id})
        if result:
            return dict(zip(columns[1:], result))
        
        try:
            response = urllib2.urlopen("http://api.tumblr.com/v2/blog/%s/posts?api_key=%s&id=%s"
                % (hostname, tumblr_api_key, id))
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e
            
        post = json.load(response)["response"]["posts"][0]
            
        original = None
        large = None
        thumb = None
        video = ""
            
        type = post["type"]
            
        if type == "photo":
            photo = post["photos"][0]
            original = photo["original_size"]["url"]
                
            alt_sizes = photo["alt_sizes"]
            for size in alt_sizes:
                if size["width"] > 420 and size["width"] <= 500:
                    large = size["url"]
                
            if large is None:
                large = original
                
            thumb = alt_sizes[-2]["url"]
        elif type == "video":
            original = large = thumb = post["thumbnail_url"]
            video = post["video_url"] if "video_url" in post else ""
        else:
            raise IsNotPictureError()

        self.insert_all(cursor, table, (id, original, large, thumb, video))
        return dict(zip(columns[1:], (original, large, thumb, video)))
    
    def get_full(self, match):
        return self.work(match)["original"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)
    
    def get_large(self, match):
        return self.work(match)["large"]

    def get_large_https(self, match):
        return self.get_large(match).replace("http://", "https://", 1)
    
    def get_thumb(self, match):
        return self.work(match)["thumb"]

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return self.work(match)["video"]

    def get_video_https(self, match):
        return self.get_video(match).replace("http://", "https://", 1)
