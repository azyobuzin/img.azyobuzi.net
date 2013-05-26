# -*- coding: utf-8 -*-

from resolvers import *

class Twipple(Resolver):
    @property
    def service_name(self):
        return u"ついっぷるフォト"
    
    @property
    def regex_str(self):
       return r"^http://(?:p.twipple\.jp|(?:p.twipple\.jp|p.twpl.jp)/show/\w+)/(\w+)(?:\?.*)?$"

    def get_full(self, match):
        return "http://p.twpl.jp/show/orig/" + match.group(1)

    def get_full_https(self, match):
        return None
    
    def get_large(self, match):
        return "http://p.twipple.jp/show/large/" + match.group(1)

    def get_large_https(self, match):
        return None
    
    def get_thumb(self, match):
        return "http://p.twipple.jp/show/thumb/" + match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
