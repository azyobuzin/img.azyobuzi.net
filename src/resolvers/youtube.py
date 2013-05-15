# -*- coding: utf-8 -*-

from resolvers import *

class YouTube(Resolver):
    @property
    def service_name(self):
        return "YouTube"
    
    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?(?:youtube\.com/watch/?\?(?:.+&)?v=([\w\-]+)(?:&.*)?|youtu\.be/([\w\-]+)/?(?:\?.*)?)$"
    
    def get_full(self, match):
        group1 = match.group(1)
        return "http://i.ytimg.com/vi/%s/0.jpg" % (group1 if group1 else match.group(2))

    def get_full_https(self, match):
        group1 = match.group(1)
        return "https://i.ytimg.com/vi/%s/0.jpg" % (group1 if group1 else match.group(2))
    
    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)
    
    def get_thumb(self, match):
        group1 = match.group(1)
        return "http://i.ytimg.com/vi/%s/default.jpg" % (group1 if group1 else match.group(2))

    def get_thumb_https(self, match):
        group1 = match.group(1)
        return "https://i.ytimg.com/vi/%s/default.jpg" % (group1 if group1 else match.group(2))

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
