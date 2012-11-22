# -*- coding: utf-8 -*-

import re

class aahosyu:
    def __str__(self):
        return u"AA保守！(･∀･"

    regexStr = "^http://aahosyu\\.com/aa/(\\d+)/?(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    def getFullSize(self, match):
        return "http://aahosyu.com/c/%s.png" % match.group(1)

    def getLargeSize(self, match):
        return "http://aahosyu.com/c/%s.png" % match.group(1)

    def getThumbnail(self, match):
        return "http://aahosyu.com/c/%s.png" % match.group(1)
