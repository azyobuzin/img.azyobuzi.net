# -*- coding: utf-8 -*-

import re
import urllib2

from resolvers import *

class Piapro(StoringResolver):
    @property
    def service_name(self):
        return "PIAPRO"
    
    @property
    def regex_str(self):
       return r"^https?://(?:www\.)?piapro\.jp/t/(\w+)(?:\?.*)?$"
    
    image_regex = re.compile(r"""<div id="_image" class="dtl_works dtl_ill" style="background:url\((http://[a-z0-9/_\\-\\.]+)_0740_0500\.jpg\) no-repeat center;">""", re.IGNORECASE)
    
    def get_parameters(self, match):
        return match.group(1)
    
    def _work(self, param, cursor):

        table = "piapro"
        columns = ["id", "prefix"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]
                    
        try:
            response = urllib2.urlopen("http://piapro.jp/t/" + param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        html = response.read().decode("utf-8")
        match = self.image_regex.search(html)
        if not match:
            raise IsNotPictureError()

        prefix = match.group(1)
        
        self.insert_all(cursor, table, (param, prefix))
        return prefix
    
    def get_full(self, match):
        return self.work(match) + "_0740_0500.jpg"

    def get_full_https(self, match):
        return None
    
    def get_large(self, match):
        return self.work(match) + "_0500_0500.jpg"

    def get_large_https(self, match):
        return None
    
    def get_thumb(self, match):        
        return self.work(match) + "_0120_0120.jpg"

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
