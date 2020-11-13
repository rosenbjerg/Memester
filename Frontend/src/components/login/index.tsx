import { Component, h } from "preact";
import * as style from "./style.css";

export default class PopUp extends Component {
    handleClick = () => {
       // this.props.toggle();
    };

    render() {
        return (
            <div class={style.modal}>
                <div class="modal_content">
          <span class="close" onClick={this.handleClick}>
            &times;
          </span>
                    <form>
                        <div>LOGIN/REGISTER</div>
                        <label>
                            email:
                            <input type="text" name="email" />
                        </label>
                        <br />
                        <input class={style.submit} type="submit" value={"SEND LOGIN LINK"} />
                    </form>
                </div>
            </div>
        );
    }
}
