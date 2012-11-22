# -*- coding: utf-8 -*-

import re

class molome:
    def __str__(self):
        return "MOLOME"

    regexStr = "^https?://(?:www\\.)?molo\\.me/p/(\\w+)/?(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    def getFullSize(self, match):
        return "http://p.molo.me/" + match.group(1)

    def getLargeSize(self, match):
        return "http://p480x480.molo.me/%s_480x480" % match.group(1)

    def getThumbnail(self, match):
        return "http://p135x135.molo.me/%s_135x135" % match.group(1)
