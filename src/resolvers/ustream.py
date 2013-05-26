# -*- coding: utf-8 -*-

import json
import urllib
import urllib2

from resolvers import *
from resolvers.private_constant import *

class Ustream(StoringResolver):
    @property
    def service_name(self):
        return "Ustream.tv"
    
    @property
    def regex_str(self):
        return r"^(?:https?://(?:www\.)?ustream\.tv/channel/(?:([\w\-%]+)|id/(\d+))|http://ustre\.am/(\w+)\)?)/?(?:\?.*)?$"
    
    def get_parameters(self, match):
        channel = match.group(1)
        shorten = match.group(3)
        
        if shorten:
            #http://azyobuzin.hatenablog.com/entry/2012/09/16/210350
            alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            expanded = sum([len(alphabet) ** i * alphabet.index(shorten[len(shorten) - 1 - i]) for i in range(len(shorten))])

        return {
                   "channel": urllib.unquote(channel).decode("utf-8") if channel else None,
                   "id": expanded if shorten else match.group(2)
               }
    
    def _work(self, param, cursor):
        table = "ustream"
        columns = ["channel", "id", "small", "medium"]
        cond = {"channel": param["channel"]} if param["channel"] else {"id": param["id"]}
        result = self.select_one(cursor, table, columns[2:], cond)
        if result:
            return dict(zip(columns[2:], result))
        
        response = urllib2.urlopen("http://api.ustream.tv/json/channel/%s/getInfo?key=%s"
            % (urllib2.quote(param["channel"] if param["channel"] else param["id"]), ustream_api_key))
            
        results = json.load(response)["results"]
            
        if results is None:
            raise PictureNotFoundError()
            
        id = results["id"]
        channel = results["urlTitleName"]
            
        imageUrl = results["imageUrl"]
        small = imageUrl["small"]
        medium = imageUrl["medium"]

        self.insert_all(cursor, table, (channel, id, small, medium))
        return dict(zip(columns[2:], (small, medium)))
    
    def get_full(self, match):
        return self.work(match)["medium"]

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
