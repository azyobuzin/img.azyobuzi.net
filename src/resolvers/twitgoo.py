# -*- coding: utf-8 -*-

from resolvers import *

class Twitgoo(Resolver):
    @property
    def service_name(self):
        return "Twitgoo"
    
    @property
    def regex_str(self):
       return r"^http://(?:www\.)?twitgoo\.com/(?:show/\w+/)?(\w+)(?:/\w*/?)?(?:\?.*)?$"
    
    def get_full(self, match):
        return "http://twitgoo.com/show/img/" + match.group(1)
    
    def get_full_https(self, match):
        return None
    
    #miniとthumbは同じ
    
    def get_large(self, match):
        return "http://twitgoo.com/show/thumb/" + match.group(1)

    def get_large_https(self, match):
        return None
    
    def get_thumb(self, match):
        return "http://twitgoo.com/show/thumb/" + match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
