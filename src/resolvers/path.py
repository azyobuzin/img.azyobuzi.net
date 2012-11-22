# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from private_constant import *

class path:
	def __str__(self):
		return "Path"

	regexStr = "^https?://(?:www\\.)?path\\.com/p/(\\w+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)

	originalRegex = re.compile("<meta property=\"og:image\" content=\"(https://[a-z0-9/_\\-\\.]+/)original(\\.\\w+)\" />", re.IGNORECASE)

	last = None

	def getUriData(self, match):
		id = match.group(1)

		if self.last is not None and str(self.last[0]) == id:
			return self.last

		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()

		c.execute("SELECT * FROM path WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()

		if sqlResult is None:
			httpRes = None

			try:
				httpRes = urllib2.urlopen("https://www.path.com/p/" + id)
			except:
				return None

			html = httpRes.read().decode("utf-8")

			match = self.originalRegex.search(html)
			prefix = match.group(1)
			extension = match.group(2)

			c.execute("INSERT INTO path VALUES (%s, %s, %s)"
				% (db.literal(id), db.literal(prefix), db.literal(extension))
			)

			db.commit()

			self.last = [id, prefix, extension]
		else:
			self.last = sqlResult

		return self.last

	def getFullSize(self, match):
		data = self.getUriData(match)

		return (data[1] + "original" + data[2]) if data is not None else None

	def getLargeSize(self, match):
		data = self.getUriData(match)

		return (data[1] + "2x" + data[2]) if data is not None else None

	def getThumbnail(self, match):
		data = self.getUriData(match)

		return (data[1] + "1x" + data[2]) if data is not None else None
