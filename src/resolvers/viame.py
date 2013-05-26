# -*- coding: utf-8 -*-

from resolvers import *

class ViaMe(Resolver): #メモ：スキーマを https にしても結局 http にリダイレクトされる
    @property
    def service_name(self):
        return "Via.Me"
    
    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?via\.me/\-(\w+)/?(?:\?.*)?$"
    
    def get_full(self, match):
        return "http://via.me/-%s/thumb/r600x600" % match.group(1)

    def get_full_https(self, match):
        return "https://via.me/-%s/thumb/r600x600" % match.group(1)
    
    def get_large(self, match):
        return "http://via.me/-%s/thumb/s300x300" % match.group(1)

    def get_large_https(self, match):
        return "https://via.me/-%s/thumb/s300x300" % match.group(1)
    
    def get_thumb(self, match):
        return "http://via.me/-%s/thumb/s150x150" % match.group(1)

    def get_thumb_https(self, match):
        return "https://via.me/-%s/thumb/s150x150" % match.group(1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
