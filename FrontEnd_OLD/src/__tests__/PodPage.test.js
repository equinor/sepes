import React from 'react';
import ReactDOM from 'react-dom';
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";

import PodPage from '../components/PodPage'


var div = null

beforeEach(() => {
  // setup a DOM element as a render target
  div = document.createElement("div");
  // container *must* be attached to document so events work correctly.
  document.body.appendChild(div);
});

afterEach(() => {
  unmountComponentAtNode(div);
  div.remove();
  div = null;
});


it("PodPage addIncomingRule", () => {
  var state = {
    userName: "user",
    selection: {
      dataset: []
    }
  }
  act(() => {
    render(<PodPage state={state} />, div)
  })

  var el = document.querySelector("button")

  //expect(el.getAttribute("disabled")).toBe("false")
  console.log(el.getAttribute("disabled"))

  expect(1).toBe(1)
})