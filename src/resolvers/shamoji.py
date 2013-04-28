# -*- coding: utf-8 -*-

from resolvers import *

class Shamoji(Resolver):
    @property
    def service_name(self):
        return "Shamoji"
    
    @property
    def regex_str(self):
        return r"^http://www\.shamoji\.info/app.php/picture/(\d+)/?(?:\?.*)?$"
    
    def get_full(self, match):
        return "http://www.shamoji.info/app.php/picture/%s/860x0.jpg" % match.group(1)
    
    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return None
    
    def get_thumb(self, match):
        return "http://www.shamoji.info/app.php/picture/%s/thumb.jpg" % match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
