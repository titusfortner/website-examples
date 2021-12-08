import os

import pytest
from selenium.webdriver import Chrome
from selenium.webdriver import Firefox
from selenium.webdriver import Remote
from selenium.webdriver.chrome.options import Options as ChromeOptions
from selenium.webdriver.common.by import By


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


def test_old_code_new_chrome():
    """Same code as above, but in Chromium 96+.
     Selenium 4.0 has same error as Selenium 3.
     Selenium 4.1 has AttributeError for using the old find_element_* method"""

    driver = Chrome()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    shadow_root = driver.execute_script('return arguments[0].shadowRoot', shadow_host)

    with pytest.raises(AttributeError, match="'ShadowRoot' object has no attribute 'find_element_by_css_selector'"):
        shadow_root.find_element_by_css_selector('#shadow_content')

    driver.quit()


def test_fix_old_code():
    """Same code as above, but using the new By class for find_element()
    This works in Selenium 4.1."""

    driver = Chrome()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element(By.CSS_SELECTOR, '#shadow_host')
    shadow_root = driver.execute_script('return arguments[0].shadowRoot', shadow_host)
    shadow_content = shadow_root.find_element(By.CSS_SELECTOR, '#shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()


def test_recommended_code():
    """Please use this code."""

    driver = Chrome()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element(By.CSS_SELECTOR, '#shadow_host')
    shadow_root = shadow_host.shadow_root
    shadow_content = shadow_root.find_element(By.CSS_SELECTOR, '#shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()


def test_firefox_workaround():
    """Firefox is special."""

    driver = Firefox()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element(By.CSS_SELECTOR, '#shadow_host')
    children = driver.execute_script('return arguments[0].shadowRoot.children', shadow_host)

    shadow_content = next(child for child in children if child.get_attribute('id') == 'shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()
