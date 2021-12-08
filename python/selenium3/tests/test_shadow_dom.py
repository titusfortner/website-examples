import os

import pytest
from selenium.webdriver import Chrome
from selenium.webdriver import Firefox
from selenium.webdriver import Remote
from selenium.webdriver.chrome.options import Options as ChromeOptions
from selenium.webdriver.remote.webelement import WebElement


def test_old_code_old_chrome():
    """What most people use for Shadow DOM Elements.
    Still works for Chrome < v96, Edge < v96, Safari"""

    options = ChromeOptions()
    options.set_capability('browserVersion', '95.0')

    sauce_options = {'username': os.environ["SAUCE_USERNAME"],
                     'accessKey': os.environ["SAUCE_ACCESS_KEY"]}
    options.set_capability('sauce:options', sauce_options)

    sauce_url = "https://ondemand.us-west-1.saucelabs.com/wd/hub"

    driver = Remote(command_executor=sauce_url, options=options)

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    shadow_root = driver.execute_script('return arguments[0].shadowRoot', shadow_host)
    shadow_content = shadow_root.find_element_by_css_selector('#shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()


def test_old_code_broken():
    """Same code as above, but in Chromium 96+ it raises an AttributeError."""

    driver = Chrome()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    shadow_root = driver.execute_script('return arguments[0].shadowRoot', shadow_host)

    with pytest.raises(AttributeError, match="'dict' object has no attribute 'find_element_by_css_selector'"):
        shadow_root.find_element_by_css_selector('#shadow_content')

    driver.quit()


def test_hack_works():
    """If you absolutely *have to use Shadow Dom Elements in latest Chromium versions
    in Selenium 3, this is the way to do it. It is hacky, so please update to Selenium 4 as soon as you can."""

    driver = Chrome()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')

    shadow_root_dict = driver.execute_script('return arguments[0].shadowRoot', shadow_host)
    shadow_root_id = shadow_root_dict['shadow-6066-11e4-a52e-4f735466cecf']
    shadow_root = WebElement(driver, shadow_root_id, w3c=True)

    shadow_content = shadow_root.find_element_by_css_selector('#shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()


def test_firefox_workaround():
    """Firefox is special."""

    driver = Firefox()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    children = driver.execute_script('return arguments[0].shadowRoot.children', shadow_host)

    shadow_content = next(child for child in children if child.get_attribute('id') == 'shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()
