# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import xml.etree.ElementTree as ET
from private_constant import *

class tinami:
	def __str__(self):
		return "TINAMI"

	regexStr = "^https?://(?:www\\.)?tinami\\.(?:com|jp)/(?:view/(\\d+)/?|([a-z0-9]+))(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)

	last = None

	def getUriData(self, match):
		id = match.group(1) if match.group(1) is not None else str(int(match.group(2), base = 36))

		if self.last is not None and str(self.last[0]) == id:
			return self.last

		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()

		c.execute("SELECT * FROM tinami WHERE cont_id = " + db.literal(id))
		sqlResult = c.fetchone()

		if sqlResult is None:
			httpRes = urllib2.urlopen("http://api.tinami.com/content/info?api_key=%s&cont_id=%s"
				% (tinamiApiKey, id))

			root = ET.fromstring(httpRes.read())
			content = root.find("content")

			if content is None:
				return None

			thumbnail = list(content.find("thumbnails"))[0].get("url")

			images = content.find("images")
			image = (images if images != None else content).find("image").find("url").text

			c.execute("INSERT INTO tinami VALUES (%s, %s, %s)"
				% (db.literal(id), db.literal(thumbnail), db.literal(image))
			)

			db.commit()

			self.last = [id, thumbnail, image]
		else:
			self.last = sqlResult

		return self.last

	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None

	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None

	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
